using System.Security.Cryptography;
using System.Text;
using Blog.Shared;
using Blog.Shared.Enums;

namespace Blog.Domain.UserDomain.Entity;

public class User : AggregateRoot<int>
{

    /// <summary>
    /// 电子邮箱
    /// </summary>
    public string Email { get; set; }
    /// <summary>
    /// 密码
    /// </summary>
    public string Password { get; private set; }
    /// <summary>
    /// 昵称
    ///  </summary>
    public string Nickname { get;private set; }
    /// <summary>
    /// 头像
    /// </summary>
    public string Avatar { get; set; }
    /// <summary>
    /// 性别
    /// </summary>
    public int Gender { get; set; }
    /// <summary>
    /// 简介
    /// </summary>
    public string? SimpleIntroduction { get; set; }
    /// <summary>
    /// 生日
    /// </summary>
    public DateOnly? BirthDay { get;private set; }
    /// <summary>
    /// 地区
    /// </summary>
    public string? Region { get; set; }
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime? CreateTime { get; set; }
    /// <summary>
    /// 上次登录
    /// </summary>
    public DateTime? LastLoginTime { get; set; }
    /// <summary>
    /// 状态
    /// </summary>
    public int Status { get; set; } = (int)UserStatus.Normal;
    
    public void EncryptPassword(string password)
    {
        Password = GetEncryptedStr(password);
    }

    public void SetEncryptedPassword(string password)
    {
        Password = password;
    }
    
    public bool ComparePassword(string encryptedPassword)
    {
        return Password == encryptedPassword;
    }

    public void SetBirthDay(DateTime dateTime)
    {
        BirthDay = DateOnly.FromDateTime(dateTime);
    }

    private static string GetEncryptedStr(string str)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(str));
        var encryptedPwd = BitConverter.ToString(bytes).Replace("-", string.Empty);
        return encryptedPwd;
    }
    
    public void SetNickname(string nickname)
    {
        Nickname = nickname;
    }

    public void SetDefaultNickname()
    {
       var index = Email.IndexOf("@", StringComparison.Ordinal);
       if(index < 0)
           throw new InvalidOperationException("无效的电子邮箱地址");
       Nickname = nameof(User).ToLower() + Email[..index];
    }
}