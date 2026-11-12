using Blog.Shared;
using Blog.Shared.Functional;

namespace Blog.Domain.UserDomain.Entity;

public class UserOptions : AggregateRoot<string>
{
    public UserOptions()
    {
        Id = RandomGenerator.Guid();
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