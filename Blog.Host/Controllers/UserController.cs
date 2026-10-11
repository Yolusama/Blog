using Blog.Application.Attributes;
using Blog.Application.Services.UserDomain.Interfaces;
using Blog.Domain.UserDomain.DTO;
using Blog.Domain.UserDomain.ValueObjects;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Controllers;

[ApplicationService]
public class UserController(IUserLoginService userLoginService) : ControllerBase
{
    [HttpPost]
    [UnitOfWork]
    public async Task<UserAuthVO> Login([FromBody] UserLoginDTO loginDto,[FromQuery] bool useVerificationCode)
    {
        return await userLoginService.Login(loginDto,useVerificationCode);
    }
    
    [HttpPut]
    [UnitOfWork]
    public async Task Register([FromBody] UserRegisterDTO registerDto)
    {
        await userLoginService.Register(registerDto);
    }
    
    [HttpGet]
    public async Task<string> GetVerificationCode([FromQuery] string email,[FromQuery] int codeLength)
    {
        return await userLoginService.GetVerificationCode(email, codeLength);
    }
    
    [HttpGet]
    public async Task<bool> UserExists([FromQuery] string email)
    {
        return await userLoginService.UserExists(email);
    }
    
    [HttpPost]
    public async Task<bool> VerifyToken([FromBody] UserVerifyTokenDTO verifyTokenDto)
    {
        return await userLoginService.VerifyToken(verifyTokenDto);
    }
    
    [HttpPost("{userId}")]
    public async Task Logout([FromRoute] int userId)
    {
        await userLoginService.Logout(userId);
    }
}