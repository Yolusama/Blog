using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Blog.MQ.Core.Attributes;
using Blog.MQ.Core.Request;
using Blog.MQ.Model.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Blog.MQ.Core;

using MQActionHandler = Func<MQControllerBase,object[],object?>;
public class MQConsumer(ILogger<MQConsumer> logger,
    IConnection connection,
    RabbitMQConfig config,
    IServiceProvider services) : IDisposable
{
    private const int MaxRetryCount = 10;
    private const string RetryCountHeader = "x-retry-count";
    private const ushort DefaultPrefetchCount = 10;
    private readonly SemaphoreSlim _consumeGate = new(1, 1);
    private readonly HashSet<string> _consumingQueues = [];
    private IChannel? _channel;
    private AsyncEventingBasicConsumer? _consumer;
    private readonly ConcurrentDictionary<string,MethodInfo> _routeHandlers = [];
    private readonly ConcurrentDictionary<string,MQActionHandler> _handlersCache = [];
    private readonly JsonSerializerSettings _serializerSettings = new()
    {
        ContractResolver = new CamelCasePropertyNamesContractResolver()
    };
    
    public async Task Register()
    {
        _channel = await connection.CreateChannelAsync();
        _consumer = new AsyncEventingBasicConsumer(_channel);
        _consumer.ReceivedAsync += HandleReceivedAsync;
        const string Controller = nameof(Controller);
        var types = Assembly.GetExecutingAssembly().GetTypes();
        var controllerTypes = types.Where(t => t.GetCustomAttribute<MQControllerAttribute>() != null);
        foreach (var controllerType in controllerTypes)
        {
            var controllerAttr = controllerType.GetCustomAttribute<MQControllerAttribute>();
            var typeName = controllerType.Name;
            var pattern = string.IsNullOrEmpty(controllerAttr.Pattern) 
                ? (typeName.EndsWith(Controller) ? typeName.Substring(0, typeName.Length - Controller.Length) : typeName)
                : controllerAttr.Pattern;
            var queue = string.IsNullOrEmpty(controllerAttr.QueueName)
                ? (typeName.EndsWith(Controller)
                    ? typeName.Substring(0, typeName.Length - Controller.Length)
                    : typeName)
                : controllerAttr.QueueName;
            var actions = controllerType.GetMethods()
                .Where(m => m.GetCustomAttribute<NoneMQAction>() == null);
            foreach (var action in actions)
            {
                var actionAttr = action.GetCustomAttribute<MQActionAttribute>();
                var routeKey = !string.IsNullOrEmpty(actionAttr?.Route) ? actionAttr.Route : action.Name;
                var route = $"{pattern}.{routeKey}";
                if(_routeHandlers.TryAdd(route,action))
                    await DeclareMessageHandler(queue,
                        config.ClientProvideName, route);
            }
 
        }
    }

    private async Task DeclareMessageHandler(string queueName, string exChange, string routeKey)
    {
        try
        {
            await _consumeGate.WaitAsync();
            try
            {
                if (_consumingQueues.Contains(queueName))
                {
                    return;
                }
                if (_channel is null || _consumer is null)
                {
                    throw new InvalidOperationException("RabbitMQ consumer channel is not initialized.");
                }

                await _channel.ExchangeDeclareAsync(exChange, ExchangeType.Direct, durable: true, autoDelete: false);
                await _channel.QueueDeclareAsync(queueName, durable: true, exclusive: false, autoDelete: false);
                await _channel.QueueBindAsync(queueName, exChange, routeKey);
                await _channel.BasicQosAsync(0, DefaultPrefetchCount, false);
                await _channel.BasicConsumeAsync(queueName, autoAck: false, consumer: _consumer);
                _consumingQueues.Add(queueName);
            }
            finally
            {
                _consumeGate.Release();
            }

            logger.LogTrace($"开始消费队列 {queueName} 的消息，交换机 {exChange}，路由键 {routeKey}");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            logger.LogError($"消费消息从队列 {queueName}，交换机 {exChange}，路由键 {routeKey} 出现异常: {e}");
            throw;
        }
    }
    public void Dispose()
    {
        _consumeGate.Dispose();
        _channel?.Dispose();
    }
    
    private async Task HandleReceivedAsync(object sender, BasicDeliverEventArgs ea)
    {
        if (_channel is null)
        {
            return;
        }
        
        var route = ea.RoutingKey;
        try
        {
            if (_routeHandlers.TryGetValue(route, out var method))
            {
                var type = method.DeclaringType;
                var controller = HandleDependencies(type);
                var controllerBase = (MQControllerBase)controller;
                controllerBase.Context = new MQRequestContext
                {
                    Route = ea.RoutingKey,
                    Exchange = ea.Exchange,
                    Body = Encoding.UTF8.GetString(ea.Body.ToArray())
                };
                var parameters = method.GetParameters();
                var values = new  object[parameters.Length];
                if (values.Length > 0)
                {
                    var body = (JObject)JsonConvert.DeserializeObject(controllerBase.Context.Body, _serializerSettings);
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        var jToken = body[parameters[i].Name];
                        values[i] = jToken?.ToObject(parameters[i].ParameterType);
                    }
                }
                var result = Invoke(route, method, controllerBase, values);
                if (result is not null && method.ReturnType == typeof(Task<>))
                {
                    var task = result as Task;
                    await task;
                }
            }
            await _channel.BasicAckAsync(ea.DeliveryTag, false);
        }
        catch (Exception e)
        {
            await HandleConsumeFailureAsync(ea, e);
        }
    }

    private async Task HandleConsumeFailureAsync(BasicDeliverEventArgs ea, Exception exception)
    {
        if (_channel is null)
        {
            return;
        }

        var retryCount = GetRetryCount(ea.BasicProperties);
        if (retryCount >= MaxRetryCount)
        {
            var body = Encoding.UTF8.GetString(ea.Body.Span);
            if (body.Length > 1024)
            {
                body = string.Concat(body.AsSpan(0, 1024), "...");
            }

            logger.LogError(exception,
                "消息处理失败且已达到最大重试次数 {MaxRetryCount}，丢弃该消息。交换机: {Exchange}，路由键: {Route}，消息内容: {Body}",
                MaxRetryCount, ea.Exchange, ea.RoutingKey, body);
            try
            {
                await _channel.BasicNackAsync(ea.DeliveryTag, false, requeue: false);
            }
            catch (Exception nackException)
            {
                logger.LogError(nackException, "丢弃消息时拒绝确认失败，路由键: {Route}", ea.RoutingKey);
            }

            return;
        }

        var nextRetryCount = retryCount + 1;
        logger.LogWarning(exception,
            "消息处理失败，第 {RetryCount}/{MaxRetryCount} 次重试。路由键: {Route}",
            nextRetryCount, MaxRetryCount, ea.RoutingKey);

        try
        {
            var headers = ea.BasicProperties.Headers is null
                ? new Dictionary<string, object?>()
                : new Dictionary<string, object?>(ea.BasicProperties.Headers);
            headers[RetryCountHeader] = nextRetryCount;

            var properties = new BasicProperties(ea.BasicProperties)
            {
                Headers = headers
            };

            // 带上重试次数重新投递，避免 nack 重新入队后丢失计数导致无限重试
            await _channel.BasicPublishAsync(ea.Exchange, ea.RoutingKey, mandatory: false,
                basicProperties: properties, body: ea.Body);
            await _channel.BasicAckAsync(ea.DeliveryTag, false);
        }
        catch (Exception retryException)
        {
            logger.LogError(retryException, "消息重新投递失败，将消息放回队列。路由键: {Route}", ea.RoutingKey);
            try
            {
                await _channel.BasicNackAsync(ea.DeliveryTag, false, requeue: true);
            }
            catch (Exception nackException)
            {
                logger.LogError(nackException, "消息拒绝确认失败，路由键: {Route}", ea.RoutingKey);
            }
        }
    }

    private static int GetRetryCount(IReadOnlyBasicProperties properties)
    {
        if (properties.Headers is null || !properties.Headers.TryGetValue(RetryCountHeader, out var value))
        {
            return 0;
        }

        return value switch
        {
            byte[] bytes when int.TryParse(Encoding.UTF8.GetString(bytes), out var parsed) => parsed,
            int intValue => intValue,
            long longValue => (int)longValue,
            string stringValue when int.TryParse(stringValue, out var parsed) => parsed,
            _ => 0
        };
    }

    private object HandleDependencies(Type controllerType)
    {
        var constructor = controllerType.GetConstructors()
            .Select(c => new {  c.IsPublic, Parameters = c.GetParameters(),Info = c })
            .Where(c => c.IsPublic && c.Parameters.Length > 0)
            .OrderByDescending(c => c.Parameters.Length)
            .FirstOrDefault();
        if (constructor != null)
        {
            var parameters = constructor.Parameters.AsEnumerable();
            List<object> values = [];
            foreach (var parameter in parameters)
            {
                var dependencyObject = services.GetRequiredService(parameter.ParameterType);
                values.Add(dependencyObject);
            }

            return constructor.Info.Invoke(values.ToArray());
        }

        return Activator.CreateInstance(controllerType);
    }
    
    private object? Invoke(string route,MethodInfo method, MQControllerBase instance, object[] args)
    {
        var invoker = _handlersCache.GetOrAdd(route, CreateInvoker(instance, method, args));
        return invoker(instance, args);
    }

    private static MQActionHandler CreateInvoker(MQControllerBase instance,MethodInfo method,object[] args)
    {
        var instanceParam = Expression.Parameter(typeof(object), nameof(instance));
        var argsParam = Expression.Parameter(typeof(object[]), nameof(args));

        // 目标实例：静态方法为 null，实例方法做一次强转
        Expression instanceExpr = method.IsStatic
            ? Expression.Constant(null, method.DeclaringType!)
            : Expression.Convert(instanceParam, method.DeclaringType!);

        // 逐个参数从 object[] 里取出并强转成声明类型
        var parameters = method.GetParameters();
        var argExpressions = new Expression[parameters.Length];
        for (int i = 0; i < parameters.Length; i++)
        {
            var item = Expression.ArrayIndex(argsParam, Expression.Constant(i));
            var convert = Expression.Convert(item, parameters[i].ParameterType);
            argExpressions[i] = convert;
        }

        Expression call = Expression.Call(instanceExpr, method, argExpressions);

        // 返回值统一装箱成 object，void 方法返回 null
        Expression body = method.ReturnType == typeof(void)
            ? Expression.Block(call, Expression.Constant(null, typeof(object)))
            : Expression.Convert(call, typeof(object));

        var lambda = Expression.Lambda<MQActionHandler>(
            body, instanceParam, argsParam);

        return lambda.Compile();
    }
}