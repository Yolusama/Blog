using Blog.MQ.Core.Request;

namespace Blog.MQ.Core;

public class MQControllerBase
{
    public MQRequestContext Context { get; set; }
}