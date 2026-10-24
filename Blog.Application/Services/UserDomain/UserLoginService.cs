using Blog.Application.Attributes;
using Blog.Application.Services.UserDomain.Interfaces;
using Blog.Domain.Services.UserDomain.Interfaces;
using Blog.Domain.UserDomain.DTO;
using Blog.Domain.UserDomain.Entity;
using Blog.Domain.UserDomain.Services.Interfaces;
using Blog.Domain.UserDomain.ValueObjects;
using Blog.Infrastructure.Interfaces;
using Blog.Shared;
using Blog.Shared.Enums;
using Blog.Shared.Exceptions;
using Blog.Shared.Functional;
using Blog.Shared.Utils;
using Microsoft.EntityFrameworkCore;

namespace Blog.Application.Services.UserDomain;

[ServiceInjection]
public class UserLoginService(
    ILogger<UserLoginService> logger,
    IEmailSender  emailSender,
    IRepository<User, int> userRepository,
    IJwtService jwtService,
    IRedisCache redis)
    : IUserLoginService
{
    public async Task<bool> UserExists(string email)
    {
       return await userRepository.NoTracking.AnyAsync(u => u.Email == email);
    }

    public async Task<UserAuthVO> Login(UserLoginDTO loginDto,bool useVerificationCode)
    {
        var user = await userRepository.All.FirstOrDefaultAsync(u => u.Email == loginDto.Email);
        if (user == null)
            throw new ServiceException("不存在的用户");
        if(!useVerificationCode)
        {
            if (!user.ComparePassword(loginDto.Password))
                throw new ServiceException("密码错误");
        }
        else
        {
            var key = $"{RedisCachingKeys.UserRegisterVerificationCode}_{loginDto.Email}";
            if(!await redis.ExistsAsync(key))
                throw new ServiceException("验证码已过期!");
            var verificationCode = await redis.GetAsync<string>(key);
            if(verificationCode!=loginDto.VerificationCode)
                throw new ServiceException("验证码错误!",201);
        }
        var token = jwtService.Generate(new UserJwtInfo(user.Id), Constants.LoginExpire);
        await redis.SetAsync($"{user.Id}_{RedisCachingKeys.UserToken}", token, Constants.LoginExpire);
        var vo = user.ShallowCopy<User, UserAuthVO>();
        vo.Token = token;
        vo.UserId = user.Id;
        user.LastLoginTime = DateTime.Now;
        userRepository.Update(user);
        return vo;
    }

    public async Task<bool> VerifyToken(UserVerifyTokenDTO verifyTokenDto)
    {
        var userInfo = jwtService.Parse<UserJwtInfo>(verifyTokenDto.Token);
        if(userInfo.UserId != verifyTokenDto.UserId)
            return false;
        var key = $"{verifyTokenDto.UserId}_{RedisCachingKeys.UserToken}";
        if (!await redis.ExistsAsync(key))
            return false;
        var token = await redis.GetAsync<string>(key);
        return token == verifyTokenDto.Token;
    }

    public async Task Register(UserRegisterDTO registerDto)
    {
        var key = $"{RedisCachingKeys.UserRegisterVerificationCode}_{registerDto.Email}";
        if (!await redis.ExistsAsync(key))
            throw new ServiceException("验证码已过期!");
        var code = await redis.GetAsync<string>(key);
        if (code != registerDto.VerificationCode)
            throw new ServiceException("验证码错误!");
        var user = await userRepository.NoTracking.FirstOrDefaultAsync(u => u.Email == registerDto.Email);
        if (user != null)
            throw new ServiceException("该邮箱已被注册!");
        user = new User();
        user.EncryptPassword(RandomGenerator.GenerateByAlphabet(Random.Shared.Next(8, 16)));
        user.Avatar = Constants.DefaultUserAvatar;
        user.CreateTime = DateTime.Now;
        user.Email = registerDto.Email;
        user.Gender = (int)UserGender.Male;
        user.SetDefaultNickname();
        await userRepository.AddAsync(user);
    }

    public async Task<string> GetVerificationCode(string email,int codeLength)
    {
        if(!EmailUtil.ValidateEmail(email))
            throw new ServiceException("邮箱格式不正确");
        var key = $"{RedisCachingKeys.UserRegisterVerificationCode}_{email}";
        if (await redis.ExistsAsync(key))
        {
            var expire = await redis.ExpireTimeAsync(key);
            if (expire != null && expire > DateTime.Now.AddMinutes(1))
                throw new ServiceException("验证码已发送，请1分钟后再尝试");
        }

        var code = RandomGenerator.GenerateNum(codeLength);
        await emailSender.SendAsync(email, "验证码", 
            $"您的注册验证码为：{code}，有效期为{Constants.RegisterExpire.TotalMinutes}分钟，请勿泄露给他人。");
        await redis.SetAsync(key, code, Constants.RegisterExpire);
        return code;
    }

    public async Task Logout(int userId)
    {
        var key = $"{userId}_{RedisCachingKeys.UserToken}";
        if (await redis.ExistsAsync(key))
            await redis.RemoveAsync(key);
    }
}