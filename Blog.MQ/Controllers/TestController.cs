using Blog.MQ.Core;
using Blog.MQ.Core.Attributes;

namespace Blog.MQ.Controllers;

[MQController]
public class TestController(ILogger<TestController> logger) : MQControllerBase
{
    [MQAction]
    public void Index(string message)
    {
        logger.LogInformation(message);
        Console.WriteLine($"收到消息：{message}");
    }
}