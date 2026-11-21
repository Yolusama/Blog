using Blog.Infrastructure.Interfaces;
using Blog.Infrastructure.MQ;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace Blog.Infrastructure.Expansions;

public static class RabbitMQExpansion
{
    public static async Task AddRabbitMQ(this IServiceCollection services, IConfiguration configuration)
    {
        var rabbitFactory = configuration.GetSection(Blog.Shared.Constants.RabbitMQConfigKey).Get<ConnectionFactory>();
        var connection = await rabbitFactory.CreateConnectionAsync();
        services.AddSingleton(connection);
        services.AddSingleton<IMQProducer, MQProducer>();
    }
}