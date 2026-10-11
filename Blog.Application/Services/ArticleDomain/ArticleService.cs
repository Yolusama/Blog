using Blog.Application.Services.Interfaces.ArticleDomain;
using Blog.Domain.ArticleDomain.Entity;
using Blog.Domain.ArticleDomain.DTO;
using Blog.Domain.ArticleDomain.ValueObjects;
using Blog.Domain.UserDomain.Entity;
using Blog.Infrastructure.Expansions;
using Blog.Infrastructure.Interfaces;
using Blog.Shared;
using Blog.Shared.Enums;
using Blog.Shared.Functional;
using Microsoft.EntityFrameworkCore;

namespace Blog.Application.Services.ArticleDomain;

public class ArticleService(ILogger<ArticleService> logger,
    IRepository<Article,string> articleRepository,
    IRepository<ArticleSource,long> articleSourceRepository,
    IRepository<ArticleOptions,long> articleOptionsRepository,
    IRepository<ArticleComment,long> articleCommentRepository,
    IRepository<User,int> userRepository) : IArticleService
{
    public async Task<string> SaveArticle(ArticleSaveModel model)
    {
        var article = await articleRepository.All.FirstOrDefaultAsync(a => a.Id == model.ArticleId);
        if (article == null)
        {
            article = new Article(model.IsDraft);
            model.ArticleId = article.Id;
            model.ShallowCopy(article);
            if (model.Sources.Count > 0)
            {
              var articleSources = model.Sources.ShallowCopy<ArticleSourceModel,ArticleSource>();
              await articleSourceRepository.AddRangeAsync(articleSources.ToArray());
            }
            await articleRepository.AddAsync(article);
        }
        else
        {
            model.ShallowCopy(article);
            var sourceIds = model.Sources.Select(s => s.SourceId)
                .ToList();
            var sources = articleSourceRepository.All.Where(s => s.ArticleId == model.ArticleId)
                .ToList();
            var dbSourceIds = sources.Select(s => s.Id);
            var expectIds = dbSourceIds.Except(sourceIds.Where(s => s != 0)).ToList();
            if (expectIds.Count > 0)
            {
                articleSourceRepository.DeleteRange(sources.Where(s => !expectIds.Contains(s.Id)).ToArray());
                articleSourceRepository.UpdateRange(model.Sources.Where(s => s.SourceId != 0
                                                                             && !expectIds.Contains(s.SourceId))
                    .ShallowCopy<ArticleSourceModel, ArticleSource>().ToArray());
            }

            var toInsert = model.Sources.Where(s => s.SourceId == 0)
                .ShallowCopy<ArticleSourceModel, ArticleSource>().ToArray();
            if(toInsert.Length > 0)
              await articleSourceRepository.AddRangeAsync(toInsert);
            articleRepository.Update(article);
        }
        return article.Id;
    }

    public async Task<BasePageQueryResult<UserArticleVO>> GetUserArticles(UserArticleQuery query)
    {
        var articles = await articleRepository.NoTracking.Where(a => !a.IsDeleted)
            .Join(userRepository.NoTracking.Where(e => e.Status == (int)UserStatus.Normal),
                a => a.AuthorId, u => u.Id, (a, u) => new { Article = a, User = u })
            .Join(articleOptionsRepository.NoTracking,
                au => au.Article.Id, ao => ao.ArticleId, (au, ao)
                    => new { au.Article, au.User, ArticleOptions = ao })
            .WhereIf(!string.IsNullOrEmpty(query.Search), x =>
                x.Article.Title.Contains(query.Search) || x.Article.Abstract.Contains(query.Search)
                                                       || x.User.Nickname.Contains(query.Search))
            .Where(x => x.Article.AuthorId == query.UserId && x.Article.IsDraft == query.IsDraft)
            .OrderByDescending(x => x.Article.UploadTime)
            .Page(query.Page, query.PageSize)
            .Count(out var total)
            .Select(x => new UserArticleVO
            {
                ArticleId = x.Article.Id,
                Title = x.Article.Title,
                Abstract = x.Article.Abstract,
                AuthorId = x.Article.AuthorId,
                AuthorName = x.User.Nickname,
                UploadTime = x.Article.UploadTime,
                IsDraft = x.Article.IsDraft,
                AuthorAvatar = x.User.Avatar,
                DislikeCount = x.ArticleOptions.DislikeCount,
                LikeCount = x.ArticleOptions.LikeCount,
                ViewCount = x.ArticleOptions.ViewCount
            }).ToListAsync();
        foreach (var article in articles)
        {
            article.Sources = await articleSourceRepository.NoTracking
                .Where(s => s.ArticleId == article.ArticleId)
                .ToListAsync();
            article.CommentCount = await articleCommentRepository.NoTracking
                .Where(o => o.ArticleId == article.ArticleId)
                .CountAsync();
        }
        return articles.ToPagedResult(query.Page, query.PageSize, total);
    }

    public async Task<BasePageQueryResult<UserArticleVO>> GetRecentArticles(ArticleQuery query)    
    {
        var articles = await articleRepository.NoTracking.Where(a => !a.IsDeleted)
            .Join(userRepository.NoTracking.Where(e => e.Status == (int)UserStatus.Normal),
                a => a.AuthorId, u => u.Id, (a, u) => new { Article = a, User = u })
            .Join(articleOptionsRepository.NoTracking,
                au => au.Article.Id, ao => ao.ArticleId, (au, ao)
                    => new { au.Article, au.User, ArticleOptions = ao })
            .WhereIf(!string.IsNullOrEmpty(query.Search), x =>
                x.Article.Title.Contains(query.Search) || x.Article.Abstract.Contains(query.Search)
                                                       || x.User.Nickname.Contains(query.Search))
            .OrderByDescending(x => x.Article.UploadTime)
            .Page(query.Page, query.PageSize)
            .Count(out var total)
            .Select(x => new UserArticleVO
            {
                ArticleId = x.Article.Id,
                Title = x.Article.Title,
                Abstract = x.Article.Abstract,
                AuthorId = x.Article.AuthorId,
                AuthorName = x.User.Nickname,
                UploadTime = x.Article.UploadTime,
                IsDraft = x.Article.IsDraft,
                AuthorAvatar = x.User.Avatar,
                DislikeCount = x.ArticleOptions.DislikeCount,
                LikeCount = x.ArticleOptions.LikeCount,
                ViewCount = x.ArticleOptions.ViewCount
            }).ToListAsync();
        foreach (var article in articles)
        {
            article.Sources = await articleSourceRepository.NoTracking
                .Where(s => s.ArticleId == article.ArticleId)
                .ToListAsync();
            article.CommentCount = await articleCommentRepository.NoTracking
                .Where(o => o.ArticleId == article.ArticleId)
                .CountAsync();
        }
        return articles.ToPagedResult(query.Page, query.PageSize, total);
    }
}