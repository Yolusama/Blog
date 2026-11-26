using Blog.Shared;
using Blog.Shared.Functional;

namespace Blog.Domain.CommonDomain;

/// <summary>
/// 通知信息表
/// </summary>
public class Notification : AggregateRoot<long>
{
    public Notification()
    {
        Id = RandomGenerator.SnowflakeId();
    }
    /// <summary>
    /// 内容
    /// </summary>
    public string Content { get; set; }
    /// <summary>
    /// 发送者Id
    /// </summary>
    public int? SenderId { get; set; }
    /// <summary>
    /// 接收者Id
    /// </summary>
    public int ReceiverId { get; set; }
    /// <summary>
    /// 是否已读
    /// </summary>
    public bool IsRead { get; set; }
    /// <summary>
    /// 通知类型
    /// </summary>
    public int Type { get; set; }
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime? CreateTime { get; set; }
}