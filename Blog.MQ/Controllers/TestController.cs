using Blog.MQ.Core;
using Blog.MQ.Core.Attributes;

namespace Blog.MQ.Controllers;

[MQController]
public class TestController(ILogger<TestController> logger) : MQControllerBase
{
    public struct TestA
    {
        public int A { get; set; }
        public string B { get; set; }
        public bool C { get; set; }
    }
    [MQAction]
    public void Index(string message)
    {
        logger.LogInformation(message);
        Console.WriteLine($"收到消息：{message}");
    }

    [MQAction]
    public void Test(TestA model)
    {
        logger.LogInformation("A:{0},B:{1},C:{2}",model.A, model.B, model.C);
    }
}