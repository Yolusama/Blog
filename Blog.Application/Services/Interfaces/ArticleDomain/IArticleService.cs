using Blog.Application.Attributes;
using Blog.Domain.ArticleDomain.DTO;
using Blog.Domain.ArticleDomain.Entity;
using Blog.Domain.ArticleDomain.ValueObjects;
using Blog.Shared;
using Blog.Shared.Enums;

namespace Blog.Application.Services.Interfaces.ArticleDomain;

[ServiceInjection]
public interface IArticleService
{
    public Task<string> SaveArticle(ArticleSaveModel article);
    public Task<BasePageQueryResult<UserArticleVO>> GetUserArticles(UserArticleQuery query);
    public Task<BasePageQueryResult<UserArticleVO>> GetRecentArticles(ArticleQuery query);
    public Task ViewArticle(string articleId,int userId);
    public Task LikeOrDislikeArticle(string articleId,int userId,LikeOrDislikeEnum type);
    public Task PublishArticle(string articleId);
    public Task WriteComment(ArticleCommentDTO commentDto);
    public Task LikeOrDislikeComment(long commentId,LikeOrDislikeEnum type);
}