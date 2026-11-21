namespace Blog.MQ.Core.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class MQControllerAttribute : Attribute
{
    public string Pattern { get; set; }
    public string QueueName { get; set; }
}