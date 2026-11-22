using Blog.MQ.Core;
using Blog.MQ.Functional;
using Blog.MQ.Model.Configuration;
using Blog.MQ.Services.Interfaces;
using FreeSql;
using RabbitMQ.Client;
using Serilog;
using Serilog.Events;

namespace Blog.MQ.Services;

public static class ExpansionService
{
    public static void AddConfiguration<T>(this IServiceCollection services, IConfiguration configuration) where T : class
    {
        services.AddSingleton<T>(configuration.GetSection(typeof(T).Name).Get<T>());
    }
    
    public static void AddConfiguration<T>(this IServiceCollection services, T instance) where T : class
    {
        services.AddSingleton(instance);
    }

    public static void AddSerilogService(this IServiceCollection services)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateBootstrapLogger();
        try
        {
            Log.Logger.Information("初始化Serilog日志记录器...");
            services.AddSerilog((provider, lc) =>
            {
                lc.MinimumLevel.Information()
                    .WriteTo.Console()
                    .WriteTo.File("Logs/info-.log", LogEventLevel.Information, rollingInterval: RollingInterval.Hour)
                    .WriteTo.File("Logs/error-.log", LogEventLevel.Error, rollingInterval: RollingInterval.Hour)
                    .WriteTo.File("Logs/debug-.log", LogEventLevel.Debug, rollingInterval: RollingInterval.Hour);
            });
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    public static async Task AddRabbitMQ(this IServiceCollection services,IConfiguration configuration)
    {
        var config = configuration.GetSection(nameof(RabbitMQConfig)).Get<RabbitMQConfig>();
        services.AddSingleton(config);
        var connectionFactory = config.ShallowCopy(new ConnectionFactory());
        var connection =await connectionFactory.CreateConnectionAsync();
        services.AddSingleton(connection);
        services.AddSingleton<MQConsumer>();
        services.AddHostedService<MQConsumeService>();
    }

    public static void AddFreeSql(this IServiceCollection services, IConfiguration configuration)
    {
        var builder = new FreeSqlBuilder();
        var freeSql = builder
            .UseConnectionString(DataType.MySql, configuration["DbConnectionStr"])
            .UseAdoConnectionPool(true)
            .UseMonitorCommand(cmd => Console.WriteLine($"{cmd.CommandText}"))
            .UseAutoSyncStructure(false) 
            .Build();
        services.AddSingleton(freeSql);
    }

    public static void AddServices(this IServiceCollection services)
    {
        services.AddScoped<IArticleService, ArticleService>();
    }
}