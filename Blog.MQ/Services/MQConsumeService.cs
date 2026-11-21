using Blog.MQ.Core;

namespace Blog.MQ.Services;

public class MQConsumeService(MQConsumer mqConsumer) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await mqConsumer.Register();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        mqConsumer.Dispose();
        return Task.CompletedTask;
    }
}