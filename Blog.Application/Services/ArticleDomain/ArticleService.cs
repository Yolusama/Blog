using System.Linq.Expressions;
using System.Net;
using Blog.Application.Attributes;
using Blog.Application.Services.Interfaces.ArticleDomain;
using Blog.Domain.ArticleDomain.Entity;
using Blog.Domain.ArticleDomain.DTO;
using Blog.Domain.ArticleDomain.Services.Interfaces;
using Blog.Domain.ArticleDomain.ValueObjects;
using Blog.Domain.UserDomain.Entity;
using Blog.Infrastructure.Expansions;
using Blog.Infrastructure.Interfaces;
using Blog.Shared;
using Blog.Shared.Enums;
using Blog.Shared.Exceptions;
using Blog.Shared.Functional;
using Blog.Shared.Utils;
using Microsoft.EntityFrameworkCore;

namespace Blog.Application.Services.ArticleDomain;

[ServiceInjection]
public class ArticleService(ILogger<ArticleService> logger,
    IConfiguration configuration,
    IArticleSourceHandler articleSourceHandler,
    IRepository<Article,string> articleRepository,
    IRepository<ArticleSource,long> articleSourceRepository,
    IRepository<ArticleOptions,long> articleOptionsRepository,
    IRepository<ArticleComment,long> articleCommentRepository,
    IRepository<ArticleCommentOptions,long> articleCommentOptionsRepository,
    IRepository<User,int> userRepository) : IArticleService
{
    public async Task<ArticleSaveVO> SaveArticle(ArticleSaveModel model)
    {
        if(!model.IsDraft) throw new ServiceException("无法再保存已发布的文章");
        var article = await articleRepository.NoTracking.FirstOrDefaultAsync(a => a.Id == model.ArticleId);
        if (article == null)
        {
            article = new Article(model.IsDraft);
            article.AuthorId = model.UserId;
            article.Cover = Constants.DefaultArticleCover;
            article.Content = string.Empty;
            model.ArticleId = article.Id;
            if (string.IsNullOrEmpty(model.Title))
            {
                var draftCount =await articleRepository.NoTracking.Where(a=>!a.IsDeleted && a.IsDraft)
                    .Where(a=>a.AuthorId == model.UserId)
                    .CountAsync();
                if (draftCount >= Constants.UserMaxDraftCount)
                    throw new ServiceException("用户最多拥有五份草稿！",411);
                model.Title = Constants.Untitled;
            }
            model.ShallowCopy(article);
            await articleRepository.AddAsync(article);
        }
        else
        {
            // 保存前先取出原内容，用于对比 Markdown 中的内嵌资源
            var originalContent = model.ArticleOriginalContent;
            if (string.IsNullOrEmpty(originalContent))
            {
                var originalFileName = string.IsNullOrEmpty(model.Content) ? article.Content : model.Content;
                if (!string.IsNullOrEmpty(originalFileName))
                {
                    try
                    {
                        originalContent = await articleSourceHandler.ReadArticleContent(originalFileName, article.AuthorId);
                    }
                    catch (FileNotFoundException)
                    {
                        logger.LogWarning("读取文章 {ArticleId} 原始内容失败，跳过资源对比", article.Id);
                    }
                }
            }

            model.ShallowCopy(article);
            // 内容未变化时无需重复写文件，也无需同步资源
            if (!string.IsNullOrEmpty(model.ArticleTrueContent) && model.ArticleTrueContent != originalContent)
            {
                var fileName = string.IsNullOrEmpty(model.Content) ? RandomGenerator.SnowflakeId() + Constants.MarkDownFileExtension : model.Content;
                article.Content = await articleSourceHandler.StoreArticleContent(fileName, article.AuthorId, model.ArticleTrueContent);
                await SyncArticleSources(article.Id, article.AuthorId, originalContent, model.ArticleTrueContent);
            }

            articleRepository.Update(article);
        }
        return new ArticleSaveVO { ArticleId = article.Id, Content = article.Content };
    }

    public async Task<BasePageQueryResult<UserArticleVO>> GetUserArticles(UserArticleQuery query)
    {
        var articleCommentQuery = articleCommentRepository.NoTracking
            .GroupBy(ac => ac.ArticleId)
            .Select(g => new { ArticleId = g.Key, ArticleCount = g.Count() });
        var articles = await articleRepository.NoTracking.Where(a => !a.IsDeleted)
            .WhereIf(query.IsDeleted.HasValue, a => a.IsDeleted == query.IsDeleted)
            .WhereIf(query.IsDraft.HasValue, a => a.IsDraft == query.IsDraft)
            .WhereIf(query.PublishStatus.HasValue, a => a.PublishStatus == query.PublishStatus)
            .Join(userRepository.NoTracking.Where(e => e.Status == (int)UserStatus.Normal),
                a => a.AuthorId, u => u.Id, (a, u) => new { Article = a, User = u })
            .GroupJoin(articleCommentQuery,e=>e.Article.Id,ac=>ac.ArticleId,(e,ac)=>new {e.Article,e.User,Comments = ac})
            .SelectMany(e=>e.Comments.DefaultIfEmpty(),(e,ac)=>new {e.Article,e.User,Comment=ac})
            .WhereIf(!string.IsNullOrEmpty(query.Search), x =>
                x.Article.Title.Contains(query.Search) || x.Article.Abstract.Contains(query.Search)
                                                       || x.User.Nickname.Contains(query.Search))
            .Where(x => x.Article.AuthorId == query.UserId)
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
                ViewCount = x.Article.ViewCount,
                CollectCount = x.Article.CollectCount,
                CreateTime = x.Article.CreateTime,
                CommentCount = (int?)x.Comment.ArticleCount ?? 0
            }).ToListAsync();
        
        return articles.ToPagedResult(query.Page, query.PageSize, total);
    }

    public async Task<BasePageQueryResult<UserArticleVO>> GetRecentArticles(ArticleQuery query)
    {
        var articleCommentQuery = articleCommentRepository.NoTracking
            .GroupBy(ac => ac.ArticleId)
            .Select(g => new { ArticleId = g.Key, ArticleCount = g.Count() });
        var articles = await articleRepository.NoTracking.Where(a => !a.IsDeleted && !a.IsDraft
                && a.PublishStatus == (int)ArticlePublishStatus.Published)
            .Join(userRepository.NoTracking.Where(e => e.Status == (int)UserStatus.Normal),
                a => a.AuthorId, u => u.Id, (a, u) => new { Article = a, User = u })
            .GroupJoin(articleCommentQuery,e=>e.Article.Id,ac=>ac.ArticleId,(e,ac)=>
                new{e.Article,e.User,Comments = ac})
            .SelectMany(e=>e.Comments.DefaultIfEmpty(),(e,ac)=>new{e.Article,e.User,Comment=ac})
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
                ViewCount = x.Article.ViewCount,
                CollectCount = x.Article.CollectCount,
                CreateTime = x.Article.CreateTime,
                CommentCount = (int?)x.Comment.ArticleCount ?? 0
            }).ToListAsync();
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
        if (!string.IsNullOrEmpty(article.Content))
        {
            model.ArticleTrueContent = await articleSourceHandler.ReadArticleContent(article.Content, article.AuthorId);
            // 原内容用作下次保存时比较内嵌资源的基线
            model.ArticleOriginalContent = model.ArticleTrueContent;
        }
        article.ShallowCopy(model, (src, dest) =>
        {
            dest.UserId = src.AuthorId;
        });
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

    public async Task RemoveArticle(string articleId,bool trueDelete = false)
    {
        var article =await articleRepository.All.FirstOrDefaultAsync(a => a.Id == articleId);
        if (article == null)
            throw new ServiceException("文章已不存在", 404);
        if ((article.IsDraft && article.SavedWhenInit) || trueDelete)
        {
            await RemoveArticleAllContent(article);
            await articleSourceHandler.RemoveArticleFile(article.Content, article.AuthorId);
        }
        else
        {
            article.IsDeleted = true;
            article.UpdateTime = DateTime.Now;
            articleRepository.Update(article);
        }
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
        switch (type)
        {
            case LikeOrDislikeEnum.Like:
                article.Like();
                articleOptions.DisLiked = false;
                articleOptions.Liked = true;
                break;
            case LikeOrDislikeEnum.Dislike:
                article.Dislike();
                articleOptions.Liked = false;
                articleOptions.DisLiked = true;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }

        articleOptions.UpdateTime = DateTime.Now;
    }

    public async Task PublishArticle(string articleId)
    {
        var article = await articleRepository.All.FirstOrDefaultAsync(a => a.Id == articleId && !a.IsDeleted && a.IsDraft);
        if (article == null)
            throw new ServiceException("文章已不存在");
        article.WhenUpload();
    }

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

    public async Task CollectArticle(string articleId, int userId, bool toCollect)
    {
        var article = await articleRepository.NoTracking.FirstOrDefaultAsync(a => a.Id == articleId);
        if (article == null)
            throw new ServiceException("文章不存在", (int)HttpStatusCode.NotFound);

        bool optionsInit = false;
        var options = await articleOptionsRepository.NoTracking.FirstOrDefaultAsync(o => o.ArticleId == articleId && o.UserId == userId);
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
        var options = await articleOptionsRepository.NoTracking.FirstOrDefaultAsync(o => o.ArticleId == article.Id);
        if(options != null)
            articleOptionsRepository.Delete(options);
        var comments = await articleCommentRepository.NoTracking.Where(ac=>ac.ArticleId == article.Id).ToArrayAsync();
        if(comments.Length > 0 )
            articleCommentRepository.DeleteRange(comments);
        var sources = await articleSourceRepository.NoTracking.Where(ac => ac.ArticleId == article.Id).ToArrayAsync();
        if (sources.Length > 0)
        {
            foreach (var source in sources)
            {
                // 外部超链接没有本地文件，只删除记录
                if (!source.IsHyperLink && !string.IsNullOrEmpty(source.FileName))
                    await articleSourceHandler.RemoveSource(source.FileName, article.AuthorId);
            }
            articleSourceRepository.DeleteRange(sources);
        }
    }

    /// <summary>
    /// 同步文章内嵌资源：
    /// 1.删除原内容中存在、但新内容中已不再引用的资源；
    /// 2.新增新内容中引用、但原本不存在的资源记录
    /// </summary>
    /// <param name="articleId">文章Id</param>
    /// <param name="authorId">作者Id</param>
    /// <param name="originalContent">原 Markdown 内容</param>
    /// <param name="currentContent">新 Markdown 内容</param>
    private async Task SyncArticleSources(string articleId, int authorId, string? originalContent, string? currentContent)
    {
        await RemoveUnusedArticleSources(articleId, authorId, originalContent, currentContent);
        await AddMissingArticleSources(articleId, authorId, originalContent, currentContent);
    }

    /// <summary>
    /// 删除原内容中存在、但新内容中已不再引用的内嵌资源
    /// （本地资源同时删除物理文件，外部超链接仅删除记录）
    /// </summary>
    /// <param name="articleId">文章Id</param>
    /// <param name="authorId">作者Id</param>
    /// <param name="originalContent">原 Markdown 内容</param>
    /// <param name="currentContent">新 Markdown 内容</param>
    private async Task RemoveUnusedArticleSources(string articleId, int authorId, string? originalContent, string? currentContent)
    {
        var removedResources = MarkdownResourceUtil.ExtractRemovedResources(originalContent, currentContent);
        if (removedResources.Count == 0)
            return;

        var removedFileNames = removedResources
            .Where(r => !r.IsHyperLink && !string.IsNullOrEmpty(r.FileName))
            .Select(r => r.FileName!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var removedUrls = removedResources
            .Where(r => r.IsHyperLink)
            .Select(r => r.Url)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var sources = await articleSourceRepository.NoTracking
            .Where(s => s.ArticleId == articleId
                        && ((!s.IsHyperLink && s.FileName != null && removedFileNames.Contains(s.FileName))
                            || (s.IsHyperLink && s.Url != null && removedUrls.Contains(s.Url))))
            .ToArrayAsync();
        foreach (var source in sources)
        {
            // 外部超链接没有本地文件，只需删除记录
            if (!source.IsHyperLink && !string.IsNullOrEmpty(source.FileName))
                await articleSourceHandler.RemoveSource(source.FileName, authorId);
        }
        articleSourceRepository.DeleteRange(sources);
    }

    /// <summary>
    /// 新增新内容中引用、但原本没有的资源记录
    /// （本地资源需文件真实存在，外部超链接直接以 Url 登记）
    /// </summary>
    /// <param name="articleId">文章Id</param>
    /// <param name="authorId">作者Id</param>
    /// <param name="originalContent">原 Markdown 内容</param>
    /// <param name="currentContent">新 Markdown 内容</param>
    private async Task AddMissingArticleSources(string articleId, int authorId, string? originalContent, string? currentContent)
    {
        const int maxArticleSourceUrlLength = 2000;
        var addedResources = MarkdownResourceUtil.ExtractAddedResources(originalContent, currentContent);
        if (addedResources.Count == 0)
            return;

        var existedSources = await articleSourceRepository.NoTracking
            .Where(s => s.ArticleId == articleId)
            .Select(s => new { s.FileName, s.Url, s.IsHyperLink })
            .ToArrayAsync();
        var existedFileNames = new HashSet<string>(
            existedSources.Where(s => !s.IsHyperLink && !string.IsNullOrEmpty(s.FileName)).Select(s => s.FileName!),
            StringComparer.OrdinalIgnoreCase);
        var existedUrls = new HashSet<string>(
            existedSources.Where(s => s.IsHyperLink && !string.IsNullOrEmpty(s.Url)).Select(s => s.Url!),
            StringComparer.OrdinalIgnoreCase);

        foreach (var resource in addedResources)
        {
            if (resource.IsHyperLink)
            {
                if (existedUrls.Contains(resource.Url))
                    continue;
                if (resource.Url.Length > maxArticleSourceUrlLength)
                {
                    logger.LogWarning("外部资源地址过长，已跳过登记: {ArticleId}", articleId);
                    continue;
                }

                var hyperLinkSource = new ArticleSource
                {
                    FileName = null,
                    Url = resource.Url,
                    IsHyperLink = true
                };
                hyperLinkSource.Attach(articleId);
                await articleSourceRepository.AddAsync(hyperLinkSource);
                existedUrls.Add(resource.Url);
                continue;
            }

            var fileName = resource.FileName;
            if (string.IsNullOrEmpty(fileName) || existedFileNames.Contains(fileName))
                continue;
            // 只登记真实存在的本地资源文件，避免把站内其他链接记录成资源
            if (!articleSourceHandler.SourceExists(fileName, authorId))
            {
                logger.LogDebug("资源文件 {FileName} 不存在，已跳过登记: {ArticleId}", fileName, articleId);
                continue;
            }

            var source = new ArticleSource
            {
                FileName = fileName,
                Url = null,
                IsHyperLink = false
            };
            source.Attach(articleId);
            await articleSourceRepository.AddAsync(source);
            existedFileNames.Add(fileName);
        }
    }
}