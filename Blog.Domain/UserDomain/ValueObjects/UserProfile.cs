namespace Blog.Domain.UserDomain.ValueObjects;

public class UserProfile
{
    public long UserId { get; set; }
    public string Nickname { get; set; }
    public string Email { get; set; }
    public string Avatar { get; set; }
    public int Gender { get; set; }
    public string JoinTime { get; set; }
    public int CreationCount { get; set; }
    public int FansCount { get; set; }
    public int FollowingCount { get; set; }
}