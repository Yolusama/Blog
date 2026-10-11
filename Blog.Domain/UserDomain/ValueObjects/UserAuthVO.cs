namespace Blog.Domain.UserDomain.ValueObjects;

public class UserAuthVO
{
    public string Token { get; set; }
    public string Nickname { get; set; }
    public int UserId { get; set; }
    public int Gender  { get; set; }
    public string SimpleIntroduction { get; set; }
    public string Avatar { get; set; }
}