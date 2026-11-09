using Microsoft.AspNetCore.Http;

namespace Blog.Domain.UserDomain.DTO;

public class UserProfileDTO 
{
    public int UserId { get; set; }
    public string? Nickname { get; set; }
    public string? Email { get; set; }
    public string? Avatar { get; set; }
    public int? Gender { get; set; }
    public IFormFile? AvatarFile { get; set; }
}