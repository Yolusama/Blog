using System.Net;
using Blog.Application.Attributes;
using Blog.Application.Services.Interfaces.ArticleDomain;
using Blog.Domain.ArticleDomain.DTO;
using Blog.Domain.ArticleDomain.Entity;
using Blog.Domain.UserDomain.Entity;
using Blog.Infrastructure.Interfaces;
using Blog.Shared;
using Blog.Shared.Enums;
using Blog.Shared.Exceptions;
using Blog.Shared.Functional;
using Microsoft.EntityFrameworkCore;

namespace Blog.Application.Services.ArticleDomain;

[ServiceInjection]
public class ArticleCommentService(IRepository<ArticleComment,long> articleCommentRepository,
    IRepository<ArticleCommentOptions,long> articleCommentOptionsRepository,
    IRepository<User,int> userRepository,
    ILogger<ArticleCommentService> logger) : IArticleCommentService
{
    public async Task<long> WriteComment(ArticleCommentDTO commentDto)
    {
        var comment = commentDto.ShallowCopy(new ArticleComment(commentDto.ArticleId));
        if(commentDto.RootId != Constants.EOF)
            comment.SetRootId(commentDto.RootId);
        await articleCommentRepository.InsertIdentityAsync(comment);
        return comment.Id;
    }

    public async Task LikeOrDislikeComment(long commentId, LikeOrDislikeEnum type)
    {
        var comment = await articleCommentRepository.NoTracking.Join(
                userRepository.NoTracking.Where(u => u.Status == (int)UserStatus.Normal),
                c => c.UserId, u => u.Id, (c, u) => new { Comment = c, User = u })
            .Where(x => x.Comment.Id == commentId)
            .Select(x => x.Comment)
            .FirstOrDefaultAsync();
        if (comment == null)
            throw new ServiceException("评论不存在", (int)HttpStatusCode.NotFound);
        var commentOptions = await articleCommentOptionsRepository
            .All
            .FirstOrDefaultAsync(c => 
                c.CommentId == commentId && c.UserId == comment.UserId);
        var isNewItem = false;
        if (commentOptions == null)
        {
            commentOptions = new ArticleCommentOptions(commentId, comment.UserId);
            isNewItem = true;
        }
        if (type == LikeOrDislikeEnum.Like)
        {
            commentOptions.Liked = true;
            commentOptions.Disliked = false;
            comment.Like();
        }

        if (type == LikeOrDislikeEnum.Dislike)
        {
            commentOptions.Liked = false;
            commentOptions.Disliked = true;
            comment.Dislike();
        }
        commentOptions.UpdateTime = DateTime.Now;
        
        articleCommentRepository.DbSet.Attach(comment);
        if(!isNewItem)
            articleCommentOptionsRepository.Update(commentOptions);
        else 
            await articleCommentOptionsRepository.AddAsync(commentOptions);
    }
}