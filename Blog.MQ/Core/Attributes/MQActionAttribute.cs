namespace Blog.MQ.Core.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class MQActionAttribute : Attribute
{
    public string Route { get; set; }
}