using Blog.MQ.Core.Request;

namespace Blog.MQ.Core;

public abstract class MQControllerBase
{
    public MQRequestContext Context { get; set; }
}