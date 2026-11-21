namespace Blog.Infrastructure.Interfaces;

public interface IMQProducer
{
    public Task Produce<T>(string queue, string exchange, string routeKey, T messageModel);
}