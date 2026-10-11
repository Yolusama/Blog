using Blog.Shared.Functional;

namespace Blog.Domain.Events;

public class DomainEvent
{
    public string EventId { get; }
    public string DomainFrom { get; }
    public string DomainTo { get; }
    public DateTime OccurredOn { get; }
    public string JsonData { get; }
    public bool Handled { get; set; }

    public DomainEvent(string domainFrom, string domainTo, DateTime occurredOn, string jsonData)
    {
        EventId = RandomGenerator.Guid();
        DomainFrom = domainFrom;
        DomainTo = domainTo;
        OccurredOn = occurredOn;
        JsonData = jsonData;
    }
}