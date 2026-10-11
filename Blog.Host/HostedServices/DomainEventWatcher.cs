using Blog.Domain.Events;
using Blog.Shared.Functional;

namespace Blog.HostedServices;

public class DomainEventWatcher(IServiceScopeFactory scopeFactory) : IHostedService
{
    private readonly  CancellationTokenSource _cancellationTokenSource = new();
    private IServiceScope _scope;
    private Task? _handleTask = null;
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _scope = scopeFactory.CreateScope();
        var channel = _scope.ServiceProvider.GetRequiredService<IDomainEventChannel>();

        _handleTask = HandleDomainEventsAsync(channel);
    }
    
    private async Task HandleDomainEventsAsync(IDomainEventChannel channel)
    {
        try
        {
            var channelReader = channel.CreateChannel();
            while(await channelReader.Reader.WaitToReadAsync(_cancellationTokenSource.Token))
            {
                var domainEvent = await channelReader.Reader.ReadAsync(_cancellationTokenSource.Token);
                await channel.Handle(domainEvent);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            await _cancellationTokenSource.CancelAsync();
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _cancellationTokenSource.CancelAsync();
        _scope.Dispose();
        _handleTask?.SafeDispose();
    }
}