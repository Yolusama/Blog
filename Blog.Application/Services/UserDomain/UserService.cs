using System.Net;
using Blog.Application.Attributes;
using Blog.Application.Services.UserDomain.Interfaces;
using Blog.Domain.ArticleDomain.Entity;
using Blog.Domain.UserDomain.Entity;
using Blog.Domain.UserDomain.ValueObjects;
using Blog.Infrastructure.Interfaces;
using Blog.Shared;
using Blog.Shared.Enums;
using Blog.Shared.Exceptions;
using Blog.Shared.Functional;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace Blog.Application.Services.UserDomain;

[ServiceInjection]
public class UserService(
    IConfiguration configuration,
    ILogger<UserService> logger,
    IRepository<User, int> userRepository,
    IRepository<UserOptions, string> userOptionsRepository,
    IRepository<Article, string> articleRepository) : IUserService
{
    private const string ImageBase = "Images";
    public async Task<UserProfile> GetUserProfile(int userId)
    {
        var user = await userRepository.NoTracking.Where(u => u.Status == (int)UserStatus.Normal)
            .Where(u => u.Id == userId)
            .FirstOrDefaultAsync();
        if (user == null)
            throw new ServiceException("用户不存在!", (int)HttpStatusCode.NotFound);
        var userCreationsCount = await articleRepository.NoTracking
            .Where(a => a.AuthorId == userId && a.PublishStatus == (int)ArticlePublishStatus.Published)
            .Where(a => !a.IsDeleted && !a.IsDraft)
            .CountAsync();
        return user.ShallowCopy(new UserProfile(), (src, dest) =>
        {
            dest.CreationCount = userCreationsCount;
            dest.JoinTime = src.CreateTime?.ToString("yyyy-MM-dd HH:mm:ss");
            dest.UserId = src.Id;
        });
    }

    public async Task<string> ChangeAvatar(IFormFile file,string originalName,int userId)
    {
        var path = Path.Combine(configuration[Constants.StaticFilesStorePath], ImageBase);
        if (originalName != Constants.DefaultUserAvatar)
            File.Delete(Path.Combine(path, originalName));
        var newFileName = RandomGenerator.Guid() + Path.GetExtension(file.FileName);
        var fileInfo = new FileInfo(Path.Combine(path, newFileName));
        fileInfo.CreateIfNotExists();
        using var stream = fileInfo.OpenWrite();
        await file.CopyToAsync(stream);
        var user =await userRepository.NoTracking.Where(u => u.Status == (int)UserStatus.AbNormal)
            .Where(u => u.Id == userId)
            .FirstOrDefaultAsync();
        if(user == null)
            throw new ServiceException("用户不存在！",(int)HttpStatusCode.NotFound);
        user.Avatar = newFileName;
        userRepository.Update(user);
        return newFileName;
    }
}