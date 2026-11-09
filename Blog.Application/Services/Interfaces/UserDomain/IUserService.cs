using Blog.Application.Attributes;
using Blog.Domain.UserDomain.ValueObjects;

namespace Blog.Application.Services.UserDomain.Interfaces;

[ServiceInjection]
public interface IUserService
{
    public Task<UserProfile> GetUserProfile(int userId);
    public Task<string> ChangeAvatar(IFormFile file,string originalName,int userId);
}