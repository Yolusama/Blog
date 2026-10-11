using Blog.Application.Attributes;

namespace Blog.Application.Services.UserDomain.Configuration;

[Configuration]
public class JwtConfig
{
    public string SecretKey { get; set; }
    public string Issuer { get; set; }
    public string Audience { get; set; }
}