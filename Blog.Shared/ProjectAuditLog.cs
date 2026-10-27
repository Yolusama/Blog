
namespace Blog.Shared;

public class ProjectAuditLog : AggregateRoot<long>
{
    /// <summary>
    /// Action方法名称
    /// </summary>
    public string ActionName { get; set; }
    /// <summary>
    /// Action开始到结束的执行时间（ms)
    /// </summary>
    public int ExecutionDuration { get; set; }
    /// <summary>
    /// 执行时间
    /// </summary>
    public DateTime ExecutionTime { get; set; }
    /// <summary>
    /// Action所属的请求类型
    /// </summary>
    public string ActionType { get; set; }
    /// <summary>
    /// Action描述信息
    /// </summary>
    public string ActionDesc { get; set; }
    /// <summary>
    /// Action所属的服务参数
    /// </summary>
    public string ServiceName { get; set; }
    /// <summary>
    /// 参数Json
    /// </summary>
    public string ParameterJson { get; set; }
    /// <summary>
    /// 访问路径
    /// </summary>
    public string Route { get; set; }
    /// <summary>
    /// 远程地址
    /// </summary>
    public string RemoteIpAddr { get; set; }
    /// <summary>
    /// 执行异常
    /// </summary>
    public string? Exception { get; set; }
    /// <summary>
    /// 客户端请求信息（请求头User-Agent)
    /// </summary>
    public string ClientInfo { get; set; }
}