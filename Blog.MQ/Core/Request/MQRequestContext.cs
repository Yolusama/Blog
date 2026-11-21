namespace Blog.MQ.Core.Request;

public class MQRequestContext
{
    public string Exchange { get; set; } 
    public string Route { get; set; } 
    public string Body { get; set; }
}