using Blog.Shared;

namespace Blog.Domain.UserDomain.Entity;

public class UserOptions : AggregateRoot<string>
{
    public UserOptions()
    {
        Id = Guid.NewGuid().ToString();
    }
    /// <summary>
    /// 用户Id
    /// </summary>
    public int UserId { get; set; }
    /// <summary>
    /// 用户操作对象用户Id
    /// </summary>
    public int ObjectId { get; set; }

    /// <summary>
    /// 是否已关注
    /// </summary>
    public bool IsFollowing { get; set; } = false;
}