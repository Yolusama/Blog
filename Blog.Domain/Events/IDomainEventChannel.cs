using System.Collections.Concurrent;
using System.Threading.Channels;

namespace Blog.Domain.Events;

public interface IDomainEventChannel
{
    public Channel<DomainEvent> CreateChannel();
    public Task Publish(string from,string to,string jsonData,CancellationToken token = default);
    public void Subscribe(string from,string to,Func<DomainEvent, Task> domainEventFunc);
    public Task Handle(DomainEvent domainEvent);
}