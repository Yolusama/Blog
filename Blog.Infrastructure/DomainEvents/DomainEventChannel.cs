using System.Collections.Concurrent;
using System.Threading.Channels;
using Blog.Domain.Events;
using Blog.Shared;

namespace Blog.Infrastructure.DomainEvents;

public class DomainEventChannel : IDomainEventChannel
{
    private Channel<DomainEvent> _channel;
    
    private readonly ConcurrentDictionary<(string,string), Func<DomainEvent, Task>> _subscribers = new();
    
    public async Task Publish(string from,string to, string jsonData,CancellationToken token = default)
    {
        _channel = CreateChannel();
        var domainEvent = new DomainEvent(from, to, DateTime.Now, jsonData);
        await _channel.Writer.WriteAsync(domainEvent, token);
        _channel.Writer.Complete();
    }

    public void Subscribe(string from, string to, Func<DomainEvent, Task> domainEventFunc)
    {
        _subscribers.TryAdd((from, to), domainEventFunc);
    }

    public async Task Handle(DomainEvent domainEvent)
    {
        if(_subscribers.TryGetValue((domainEvent.DomainFrom, domainEvent.DomainTo), out var handler))
            await handler.Invoke(domainEvent);
    }

    public Channel<DomainEvent> CreateChannel() => Channel
        .CreateBounded<DomainEvent>(
            new BoundedChannelOptions(Constants.ChannelCapacity
            ));
}