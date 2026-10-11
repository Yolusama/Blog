using Blog.Application.Attributes;
using Blog.Domain.UserDomain.DTO;
using Blog.Domain.UserDomain.ValueObjects;

namespace Blog.Application.Services.UserDomain.Interfaces;

[ServiceInjection]
public interface IUserLoginService
{
    public Task<bool> UserExists(string email);
    public Task<UserAuthVO> Login(UserLoginDTO loginDto,bool useVerificationCode);
    public Task<bool> VerifyToken(UserVerifyTokenDTO verifyTokenDto);
    public Task Register(UserRegisterDTO registerDto);
    public Task<string> GetVerificationCode(string email,int codeLength);
    public Task Logout(int userId);
}