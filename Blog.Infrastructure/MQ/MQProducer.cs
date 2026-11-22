using System.Text;
using System.Text.Json;
using Blog.Infrastructure.Interfaces;
using Blog.Shared.Functional;
using RabbitMQ.Client;

namespace Blog.Infrastructure.MQ;

public class MQProducer(IConnection connection) : IMQProducer
{
    private readonly JsonSerializerOptions _serializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
    public async Task Produce<T>(string queue, string exchange, string routeKey, T messageModel)
    {
        try
        {
            await using var channel = await connection.CreateChannelAsync();

            // Ensure exchange exists before bind/publish to avoid 404 NOT_FOUND.
            await channel.ExchangeDeclareAsync(
                exchange: exchange,
                type: ExchangeType.Direct,
                durable: true,
                autoDelete: false);

            await channel.QueueDeclareAsync(
                queue: queue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            await channel.QueueBindAsync(queue, exchange, routeKey);
            var message = messageModel.ToJson(_serializerOptions);
            var body = Encoding.UTF8.GetBytes(message);
            await channel.BasicPublishAsync(exchange, routeKey, body: body);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}