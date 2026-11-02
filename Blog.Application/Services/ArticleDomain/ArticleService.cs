using System.Linq.Expressions;
using System.Net;
using Blog.Application.Attributes;
using Blog.Application.Services.Interfaces.ArticleDomain;
using Blog.Domain.ArticleDomain.Entity;
using Blog.Domain.ArticleDomain.DTO;
using Blog.Domain.ArticleDomain.ValueObjects;
using Blog.Domain.UserDomain.Entity;
using Blog.Infrastructure.Expansions;
using Blog.Infrastructure.Interfaces;
using Blog.Shared;
using Blog.Shared.Enums;
using Blog.Shared.Exceptions;
using Blog.Shared.Functional;
using Microsoft.EntityFrameworkCore;

namespace Blog.Application.Services.ArticleDomain;

[ServiceInjection]
public class ArticleService(ILogger<ArticleService> logger,
    IConfiguration configuration,
    IRepository<Article,string> articleRepository,
    IRepository<ArticleSource,long> articleSourceRepository,
    IRepository<ArticleOptions,long> articleOptionsRepository,
    IRepository<ArticleComment,long> articleCommentRepository,
    IRepository<ArticleCommentOptions,long> articleCommentOptionsRepository,
    IRepository<User,int> userRepository) : IArticleService
{
    public async Task<string> SaveArticle(ArticleSaveModel model)
    {
        if(!model.IsDraft) throw new ServiceException("无法再保存已发布的文章");
        var article = await articleRepository.NoTracking.FirstOrDefaultAsync(a => a.Id == model.ArticleId);
        if (article == null)
        {
            article = new Article(model.IsDraft);
            model.ArticleId = article.Id;
            if (string.IsNullOrEmpty(model.Title))
            {
                var draftCount =await articleRepository.NoTracking.Where(a=>!a.IsDeleted && a.IsDraft)
                    .Where(a=>a.AuthorId == model.UserId)
                    .CountAsync();
                if (draftCount >= Constants.UserMaxDraftCount)
                    throw new ServiceException("用户最多拥有五份草稿！");
                model.Title = Constants.Untitled + '-' + (draftCount + 1);
            }
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
            if (article.SavedWhenInit)
                article.SavedWhenInit = false;
            var sourceIds = model.Sources.Select(s => s.SourceId)
                .ToList();
            var sources = articleSourceRepository.All.Where(s => s.ArticleId == model.ArticleId)
                .ToList();
            var dbSourceIds = sources.Select(s => s.Id);
            var expectIds = dbSourceIds.Except(sourceIds.Where(s => s != 0)).ToList();
            if (expectIds.Count > 0)
            {
                var toDelete = sources.Where(s => !expectIds.Contains(s.Id)).ToArray();
                foreach (var item in toDelete)
                {
                    if(item.IsHyperLink)continue;
                    item.Delete(configuration[Constants.StaticFilesStorePath],article.AuthorId);
                    articleSourceRepository.Delete(item);
                }
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
            .WhereIf(!string.IsNullOrEmpty(query.Search), x =>
                x.Article.Title.Contains(query.Search) || x.Article.Abstract.Contains(query.Search)
                                                       || x.User.Nickname.Contains(query.Search))
            .Where(x => x.Article.AuthorId == query.UserId && x.Article.IsDraft == query.IsDraft)
            .Count(out var total)
            .OrderByDescending(x => x.Article.UploadTime)
            .Page(query.Page, query.PageSize)
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
                DislikeCount = x.Article.DislikeCount,
                LikeCount = x.Article.LikeCount,
                ViewCount = x.Article.ViewCount
            }).ToListAsync();
        foreach (var article in articles)
        {
            article.CommentCount = await articleCommentRepository.NoTracking
                .Where(o => o.ArticleId == article.ArticleId)
                .CountAsync();
        }
        return articles.ToPagedResult(query.Page, query.PageSize, total);
    }

    public async Task<BasePageQueryResult<UserArticleVO>> GetRecentArticles(ArticleQuery query)    
    {
        var articles = await articleRepository.NoTracking.Where(a => !a.IsDeleted && !a.IsDraft
                && a.PublishStatus == (int)ArticlePublishStatus.Published)
            .Join(userRepository.NoTracking.Where(e => e.Status == (int)UserStatus.Normal),
                a => a.AuthorId, u => u.Id, (a, u) => new { Article = a, User = u })
            .WhereIf(!string.IsNullOrEmpty(query.Search), x =>
                x.Article.Title.Contains(query.Search) || x.Article.Abstract.Contains(query.Search)
                                                       || x.User.Nickname.Contains(query.Search))
            .Count(out var total)
            .OrderByDescending(x => x.Article.PublishTime)
            .Page(query.Page, query.PageSize)
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
                DislikeCount = x.Article.DislikeCount,
                LikeCount = x.Article.LikeCount,
                ViewCount = x.Article.ViewCount
            }).ToListAsync();
        foreach (var article in articles)
        {
            article.CommentCount = await articleCommentRepository.NoTracking
                .Where(o => o.ArticleId == article.ArticleId)
                .CountAsync();
        }
        return articles.ToPagedResult(query.Page, query.PageSize, total);
    }

    public async Task<ArticleSaveModel> GetArticleContent(string articleId)
    {
        var article = await articleRepository.NoTracking
            .Where(a=>a.Id == articleId && !a.IsDeleted)
            .SingleOrDefaultAsync();
        if (article == null)
            throw new ServiceException("文章已不存在",404);
        var sources = await articleSourceRepository.NoTracking
            .Where(s => s.ArticleId == articleId)
            .ToListAsync();
        var model = new ArticleSaveModel();
        model.ArticleId = article.Id;
        article.ShallowCopy(model);
        model.Sources = sources.ShallowCopy<ArticleSource,ArticleSourceModel>().ToList();
        return model;
    }

    public async Task<ArticleRankingVO> GetArticleRanking()
    { 
       var ranking = new ArticleRankingVO();
       Expression<Func<Article, bool>> filter = a => !a.IsDraft && !a.IsDeleted;
       var dailyRankingContents = await articleRepository
           .NoTracking
           .Where(a=>a.PublishTime.Value.Date == DateTime.Today)
           .Where(filter)
           .OrderByDescending(x=>x.ViewCount)
           .Take(Constants.ArticleRankingMaxCount)
           .Select(a=> new ArticleRankingVO.ArticleRankingContent
           {
               ArticleId = a.Id,
               ArticleTitle = a.Title
           })
           .ToListAsync();
       ranking.SetDaily(dailyRankingContents);
       var weeklyRankingContents = await articleRepository
           .NoTracking
           .Where(a=>a.PublishTime.Value.Date >= DateTime.Today.AddDays(-1*Constants.Week) && a.PublishTime.Value.Date <= DateTime.Today)
           .Where(filter)
           .OrderByDescending(x=>x.ViewCount)
           .Take(Constants.ArticleRankingMaxCount)
           .Select(a=> new ArticleRankingVO.ArticleRankingContent
           {
               ArticleId = a.Id,
               ArticleTitle = a.Title
           })
           .ToListAsync();
       ranking.SetWeekly(weeklyRankingContents);
       var halfMonthlyRankingContents = await articleRepository
           .NoTracking
           .Where(a=>a.PublishTime.Value.Date >= DateTime.Today.AddDays(-1*Constants.HalfMonth) && a.PublishTime.Value.Date <= DateTime.Today)
           .Where(filter)
           .OrderByDescending(x=>x.ViewCount)
           .Take(Constants.ArticleRankingMaxCount)
           .Select(a=> new ArticleRankingVO.ArticleRankingContent
           {
               ArticleId = a.Id,
               ArticleTitle = a.Title
           })
           .ToListAsync();
       ranking.SetHalfMonthly(halfMonthlyRankingContents);
       return ranking;
    }

    public async Task RemoveArticle(string articleId)
    {
        var article =await articleRepository.All.FirstOrDefaultAsync(a => a.Id == articleId);
        if (article == null)
            throw new ServiceException("文章已不存在", 404);
        if (article.IsDraft && article.SavedWhenInit)
          await  RemoveArticleAllContent(article);
        else
            article.IsDeleted = true;
        articleRepository.Update(article);
    }

    public async Task ViewArticle(string articleId, int userId)
    {
        var articleOptions = await articleOptionsRepository.NoTracking
            .FirstOrDefaultAsync(a => a.ArticleId == articleId && a.UserId == userId);
        if (articleOptions == null)
        {
            articleOptions = new ArticleOptions(articleId, userId)
            {
                Viewed = true
            };

            await articleOptionsRepository.AddAsync(articleOptions);
            var article = articleRepository.All.FirstOrDefault(a => a.Id == articleId && !a.IsDeleted && !a.IsDraft);
            if (article == null)
                throw new ServiceException("文章不存在");
            article.View();
            articleRepository.Update(article);
        }
    }

    public async Task LikeOrDislikeArticle(string articleId, int userId, LikeOrDislikeEnum type)
    {
        var articleOptions = await articleOptionsRepository.All
            .FirstOrDefaultAsync(a => a.ArticleId == articleId && a.UserId == userId);
        if (articleOptions == null)
            throw new ServiceException("错误，相关操作内容缺失");
        var article =await articleRepository.All.FirstOrDefaultAsync(a => a.Id == articleId && !a.IsDeleted && !a.IsDraft);
        if(article == null)
            throw new ServiceException("文章不存在");
        if(type == LikeOrDislikeEnum.Like)
        {
            article.Like();
            articleOptions.DisLiked = false;
            articleOptions.Liked = true;
        }
        if(type == LikeOrDislikeEnum.Dislike)
        {
            article.Dislike();
            articleOptions.Liked = false;
            articleOptions.DisLiked = true;
        }

        articleOptions.UpdateTime = DateTime.Now;
        articleRepository.Update(article);
        articleOptionsRepository.Update(articleOptions);
    }

    public async Task PublishArticle(string articleId)
    {
        var article = await articleRepository.All.FirstOrDefaultAsync(a => a.Id == articleId && !a.IsDeleted && a.IsDraft);
        if (article == null)
            throw new ServiceException("文章已不存在");
        article.WhenUpload();
        articleRepository.Update(article);
    }

    public async Task WriteComment(ArticleCommentDTO commentDto)
    {
        var comment = commentDto.ShallowCopy(new ArticleComment(commentDto.ArticleId));
        if(commentDto.RootId != Constants.EOF)
            comment.SetRootId(commentDto.RootId);
        await articleCommentRepository.AddAsync(comment);
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

    public async Task CollectArticle(string articleId, int userId, bool toCollect)
    {
        var article = await articleRepository.NoTracking.FirstOrDefaultAsync(a => a.Id == articleId);
        if (article == null)
            throw new ServiceException("文章不存在", (int)HttpStatusCode.NotFound);

        bool optionsInit = false;
        var options = await articleOptionsRepository.All.FirstOrDefaultAsync(o => o.ArticleId == articleId && o.UserId == userId);
        if (options == null)
        {
            options = new ArticleOptions(articleId, userId);
            await articleOptionsRepository.AddAsync(options);
            optionsInit = true;
        }

        if (toCollect)
        {
            article.Collect();
            options.HasCollected = true;
        }
        else
        {
            article.CancelCollect();
            options.HasCollected = false;
        }

        if (!optionsInit)
            articleOptionsRepository.Update(options);
    }

    private async Task RemoveArticleAllContent(Article article)
    {
        articleRepository.Delete(article);
        var options = await articleOptionsRepository.All.FirstOrDefaultAsync(o => o.ArticleId == article.Id);
        if(options != null)
            articleOptionsRepository.Delete(options);
        var comments = await articleCommentRepository.All.Where(ac=>ac.ArticleId == article.Id).ToArrayAsync();
        if(comments.Length > 0 )
            articleCommentRepository.DeleteRange(comments);
        var sources = await articleSourceRepository.All.Where(ac => ac.ArticleId == article.Id).ToArrayAsync();
        if (sources.Length > 0)
        {
            foreach (var source in sources.AsEnumerable())
               source.Delete(configuration[Constants.StaticFilesStorePath],article.AuthorId);
            articleSourceRepository.DeleteRange(sources);
        }
    }
}