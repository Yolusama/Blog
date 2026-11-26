using System.Linq.Expressions;
using System.Net;
using System.Runtime.Serialization;
using Blog.Application.Attributes;
using Blog.Application.Services.Interfaces.ArticleDomain;
using Blog.Application.Services.UserDomain.Configuration;
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
    RabbitMQConfig rabbitMqConfig,
    IMQProducer mqProducer,
    IRepository<Article,string> articleRepository,
    IRepository<ArticleSource,long> articleSourceRepository,
    IRepository<ArticleOptions,long> articleOptionsRepository,
    IRepository<ArticleComment,long> articleCommentRepository,
    IRepository<ArticleCommentOptions,long> articleCommentOptionsRepository,
    IRepository<User,int> userRepository) : IArticleService
{
    public async Task<ArticleSaveVO> SaveArticle(ArticleSaveModel model)
    {
        var article = await articleRepository.NoTracking.FirstOrDefaultAsync(a => a.Id == model.ArticleId);
        if (article == null)
        {
            article = new Article(model.IsDraft);
            article.AuthorId = model.UserId;
            article.Cover = Constants.DefaultArticleCover;
            article.Content = string.Empty;
            article.PublishStatus = ArticlePublishStatus.Editing.IntValue();
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
            if (string.IsNullOrEmpty(model.Title)) throw new ServiceException("请输入标题!");
            if(!article.IsDraft && article.PublishStatus == ArticlePublishStatus.Published.IntValue()) throw new ServiceException("无法再保存已发布的文章");
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
                var fileName = string.IsNullOrEmpty(article.Content)
                    ? RandomGenerator.SnowflakeId() + Constants.HTMLFileExtension
                    : model.Content;
                article.Content =
                    await articleSourceHandler.StoreArticleContent(fileName, article.AuthorId,
                        model.ArticleTrueContent);
                await SyncArticleSources(article.Id, article.AuthorId, originalContent, model.ArticleTrueContent);

                if (article.SavedWhenInit)
                    article.SavedWhenInit = false;
                article.UpdateTime = DateTime.Now;
                articleRepository.Update(article);
            }
            else throw new ServiceException("No Changes", 201);
        }
        return new ArticleSaveVO { ArticleId = article.Id, Content = article.Content };
    }

    public async Task<BasePageQueryResult<UserArticleVO>> GetUserArticles(UserArticleQuery query)
    {
        var articleCommentQuery = articleCommentRepository.NoTracking
            .GroupBy(ac => ac.ArticleId)
            .Select(g => new { ArticleId = g.Key, ArticleCount = g.Count() });
        var dbQuery = articleRepository.NoTracking
            .WhereIf(query.IsDeleted.HasValue,a=>a.IsDeleted == query.IsDeleted)
            .WhereIf(query.IsDraft.HasValue,a=>a.IsDraft == query.IsDraft)
            .WhereIf(query.PublishStatus.HasValue, a => a.PublishStatus == query.PublishStatus)
            .Join(userRepository.NoTracking.Where(e => e.Status == UserStatus.Normal.IntValue()),
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
                CommentCount = (int?)x.Comment.ArticleCount ?? 0,
                CreationDeclaration = x.Article.CreationDeclaration,
                Cover = x.Article.Cover,
                SaveWhenInit = x.Article.SavedWhenInit
            });
    
        var articles = await dbQuery.ToListAsync();
        return articles.ToPagedResult(query.Page, query.PageSize, total);
    }

    public async Task<BasePageQueryResult<UserArticleVO>> GetRecentArticles(ArticleQuery query)
    {
        var articleCommentQuery = articleCommentRepository.NoTracking
            .GroupBy(ac => ac.ArticleId)
            .Select(g => new { ArticleId = g.Key, ArticleCount = g.Count() });
        var articles = await articleRepository.NoTracking.Where(a => !a.IsDeleted && !a.IsDraft
                && a.PublishStatus == ArticlePublishStatus.Published.IntValue())
            .Join(userRepository.NoTracking.Where(e => e.Status == UserStatus.Normal.IntValue()),
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
                CreationDeclaration = x.Article.CreationDeclaration,
                Cover = x.Article.Cover,
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

    public async Task<ArticleDetailVO> GetArticleDetail(string articleId, int userId)
    {
        var data = await articleRepository.NoTracking
            .Where(a => a.Id == articleId && !a.IsDeleted)
            .Join(userRepository.NoTracking.Where(u => u.Status == UserStatus.Normal.IntValue()),
                a => a.AuthorId, u => u.Id, (a, u) => new { Article = a, User = u })
            .FirstOrDefaultAsync();
        if (data == null)
            throw new ServiceException("文章已不存在", HttpStatusCode.NotFound.IntValue());

        var article = data.Article;
        var commentCount = await articleCommentRepository.NoTracking
            .CountAsync(c => c.ArticleId == articleId);
        var detail = new ArticleDetailVO
        {
            ArticleId = article.Id,
            Title = article.Title,
            Abstract = article.Abstract,
            Cover = article.Cover,
            CreationDeclaration = article.CreationDeclaration,
            AuthorId = article.AuthorId,
            AuthorName = data.User.Nickname,
            AuthorAvatar = data.User.Avatar,
            ViewCount = article.ViewCount,
            LikeCount = article.LikeCount,
            DislikeCount = article.DislikeCount,
            CollectCount = article.CollectCount,
            CommentCount = commentCount,
            CreateTime = article.CreateTime,
            PublishTime = article.PublishTime
        };

        if (!string.IsNullOrEmpty(article.Content))
        {
            try
            {
                detail.Content = await articleSourceHandler.ReadArticleContent(article.Content, article.AuthorId);
            }
            catch (FileNotFoundException)
            {
                logger.LogWarning("读取文章 {ArticleId} 内容失败", article.Id);
            }
        }

        var options = await articleOptionsRepository.NoTracking
            .FirstOrDefaultAsync(o => o.ArticleId == articleId && o.UserId == userId);
        if (options != null)
        {
            detail.Liked = options.Liked;
            detail.Disliked = options.DisLiked;
            detail.Collected = options.HasCollected;
        }

        return detail;
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

    public async Task RecoverArticle(string articleId)
    {
        var article = await articleRepository.All.FirstOrDefaultAsync(a=>a.Id == articleId);
        if(article == null)
            throw new ServiceException("文章已不存在",(int)HttpStatusCode.NotFound);
        article.IsDeleted = true;
        article.UpdateTime = DateTime.Now;
    }

    public async Task RemoveArticle(string articleId,bool trueDelete = false)
    {
        var article =await articleRepository.All.FirstOrDefaultAsync(a => a.Id == articleId);
        if (article == null)
            throw new ServiceException("文章已不存在", 404);
        if ((article.IsDraft && article.SavedWhenInit) || trueDelete)
        {
            await RemoveArticleAllContent(article);
            if(!string.IsNullOrEmpty(article.Content))
            {
                await articleSourceHandler.RemoveArticleFile(article.Content, article.AuthorId);
                if (article.Cover != Constants.DefaultArticleCover)
                    await articleSourceHandler.RemoveArticleFile(article.Cover, article.AuthorId);
            }
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

    public async Task<string> PublishArticle(string articleId)
    {
        var article = await articleRepository.All.FirstOrDefaultAsync(a => a.Id == articleId && !a.IsDeleted && a.IsDraft
        && a.PublishStatus == ArticlePublishStatus.Editing.IntValue());
        if (article == null)
            throw new ServiceException("文章已不存在或已在审核中...");
        if (string.IsNullOrEmpty(article.Abstract?.Trim()))
            throw new SerializationException("请编辑摘要内容！");
        article.SendToBeAudited();
        //目前无人审核，直接使用MQ异步刷新状态
        await mqProducer.Produce(MQActions.ArticleQueue,rabbitMqConfig.ClientProvideName,
            MQActions.ArticleAuditAction,new {ArticleId = articleId});
        return "已发布，等待审核...页面将关闭！";
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
                // 本地资源需要同时删除物理文件（历史上被登记成超链接的站内资源也要处理）
                var fileName = GetLocalSourceFileName(source, article.AuthorId);
                if (!string.IsNullOrEmpty(fileName))
                    await articleSourceHandler.RemoveSource(fileName, article.AuthorId);
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
        var removedResources = MarkdownResourceUtil.ExtractRemovedResources(originalContent, currentContent,
            url => ResolveLocalSourceFileName(url, authorId));
        if (removedResources.Count == 0)
            return;

        // 已移除的资源可能以「文件名」（本地资源）或「Url」（超链接/历史数据）两种形式登记，
        // 两种键都收集，避免因登记形式不一致导致资源残留
        var removedKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var resource in removedResources)
        {
            if (!string.IsNullOrEmpty(resource.FileName))
                removedKeys.Add(resource.FileName);
            if (!string.IsNullOrEmpty(resource.Url))
                removedKeys.Add(resource.Url);
        }

        // 资源数量很少，直接取回内存比较，兼容「站内资源被登记成超链接」的历史数据
        var sources = await articleSourceRepository.NoTracking
            .Where(s => s.ArticleId == articleId)
            .ToArrayAsync();
        var unusedSources = sources.Where(s => IsRemovedSource(s, removedKeys)).ToArray();
        if (unusedSources.Length == 0)
            return;

        foreach (var source in unusedSources)
        {
            // 本地资源需要同时删除物理文件，外部超链接只删除记录
            var fileName = GetLocalSourceFileName(source, authorId);
            if (!string.IsNullOrEmpty(fileName))
                await articleSourceHandler.RemoveSource(fileName, authorId);
        }
        articleSourceRepository.DeleteRange(unusedSources);
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
        var addedResources = MarkdownResourceUtil.ExtractAddedResources(originalContent, currentContent,
            url => ResolveLocalSourceFileName(url, authorId));
        if (addedResources.Count == 0)
            return;

        var existedSources = await articleSourceRepository.NoTracking
            .Where(s => s.ArticleId == articleId)
            .Select(s => new { s.FileName, s.Url, s.IsHyperLink })
            .ToArrayAsync();
        // 已存在的本地资源文件名：既包含本地资源记录，也包含历史上被登记成超链接的站内资源
        var existedFileNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var existedUrls = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var source in existedSources)
        {
            if (!string.IsNullOrEmpty(source.FileName))
                existedFileNames.Add(source.FileName);
            if (string.IsNullOrEmpty(source.Url))
                continue;
            existedUrls.Add(source.Url);
            var urlFileName = ResolveLocalSourceFileName(source.Url, authorId);
            if (!string.IsNullOrEmpty(urlFileName))
                existedFileNames.Add(urlFileName);
        }

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

    /// <summary>
    /// 判断已登记的源记录是否属于「已从内容中移除」的资源。
    /// 同时兼容两种登记形式：本地资源按文件名、超链接按 Url，
    /// 以及历史上把站内静态资源登记成超链接的情况（取 Url 尾部文件名比较）。
    /// </summary>
    private static bool IsRemovedSource(ArticleSource source, HashSet<string> removedKeys)
    {
        if (!string.IsNullOrEmpty(source.FileName) && removedKeys.Contains(source.FileName))
            return true;
        if (string.IsNullOrEmpty(source.Url))
            return false;
        if (removedKeys.Contains(source.Url))
            return true;
        var urlFileName = MarkdownResourceUtil.GetFileName(source.Url);
        return !string.IsNullOrEmpty(urlFileName) && removedKeys.Contains(urlFileName);
    }

    /// <summary>
    /// 取源记录对应的本地资源文件名（用于删除物理文件）：
    /// 本地资源直接取 FileName；被登记成超链接的站内资源则从 Url 解析。
    /// </summary>
    private string? GetLocalSourceFileName(ArticleSource source, int userId)
    {
        if (!source.IsHyperLink && !string.IsNullOrEmpty(source.FileName))
            return source.FileName;
        return string.IsNullOrEmpty(source.Url) ? null : ResolveLocalSourceFileName(source.Url, userId);
    }

    /// <summary>
    /// 将站内静态资源地址解析为本地资源文件名。
    /// 上传后的资源采用静态文件访问机制，虽然以 URL 形式出现（形如
    /// http://localhost:5575/files/Source/{userId}/{fileName}），但尾部带有文件名，
    /// 命中当前用户资源目录且文件真实存在时，应按本地资源而非外部超链接处理。
    /// </summary>
    /// <param name="url">资源地址</param>
    /// <param name="userId">用户Id</param>
    /// <returns>本地资源文件名；非站内静态资源或文件不存在时返回 null</returns>
    private string? ResolveLocalSourceFileName(string url, int userId)
    {
        var requestPath = configuration[Constants.StaticFilesRequestPath];
        if (string.IsNullOrWhiteSpace(requestPath))
            return null;

        // 去掉协议与主机，只保留路径；同时兼容 //host/path 与站内相对路径
        var normalizedUrl = url.StartsWith("//", StringComparison.Ordinal) ? "http:" + url : url;
        var path = normalizedUrl;
        if (Uri.TryCreate(normalizedUrl, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            path = uri.AbsolutePath;
        var endIndex = path.IndexOfAny(new[] { '?', '#' });
        if (endIndex >= 0)
            path = path[..endIndex];
        path = path.Replace('\\', '/');

        // 仅匹配形如 {RequestPath}/Source/{userId}/{fileName} 的站内资源地址
        var sourcePrefix = $"{requestPath.TrimEnd('/')}/Source/{userId}/";
        if (!path.StartsWith(sourcePrefix, StringComparison.OrdinalIgnoreCase))
            return null;

        var fileName = MarkdownResourceUtil.GetFileName(path);
        if (string.IsNullOrEmpty(fileName))
            return null;
        // 必须是该用户资源目录下真实存在的文件，避免把外部链接误判为本地资源
        return articleSourceHandler.SourceExists(fileName, userId) ? fileName : null;
    }
}