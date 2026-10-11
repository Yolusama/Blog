using Blog.Cache;
using Blog.Infrastructure.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Blog.Infrastructure.Expansions;

public static class RedisExpansion
{
    public static void AddRedis(this IServiceCollection services, string? redisConnectionString)
    {
        var connectionMultiplexer = ConnectionMultiplexer.Connect(redisConnectionString);
        services.AddSingleton<IConnectionMultiplexer>(connectionMultiplexer);
        services.AddScoped<IRedisCache, RedisCache>();
    }
}