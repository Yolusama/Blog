using Blog.Domain.Events;
using Blog.Infrastructure.DomainEvents;
using Microsoft.Extensions.DependencyInjection;

namespace Blog.Infrastructure.Expansions;

public static class DomainEventExpansion
{
    public static void AddDomainEventChannel(this IServiceCollection services)
    {
        services.AddScoped<IDomainEventChannel, DomainEventChannel>();
    }
}