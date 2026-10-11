using Blog.Domain.ArticleDomain.DTO;
using Blog.Domain.ArticleDomain.Entity;
using Blog.Domain.ArticleDomain.ValueObjects;
using Blog.Shared;

namespace Blog.Application.Services.Interfaces.ArticleDomain;

public interface IArticleService
{
    public Task<string> SaveArticle(ArticleSaveModel article);
    public Task<BasePageQueryResult<UserArticleVO>> GetUserArticles(UserArticleQuery query);
    public Task<BasePageQueryResult<UserArticleVO>> GetRecentArticles(ArticleQuery query);
}