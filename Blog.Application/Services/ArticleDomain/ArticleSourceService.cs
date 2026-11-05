using Blog.Application.Attributes;
using Blog.Application.Services.Interfaces.ArticleDomain;
using Blog.Domain.ArticleDomain.Entity;
using Blog.Domain.ArticleDomain.Services.Interfaces;
using Blog.Infrastructure.Interfaces;
using Blog.Shared;
using Blog.Shared.Functional;

namespace Blog.Application.Services.ArticleDomain;

[ServiceInjection]
public class ArticleSourceService(
    IRepository<ArticleSource,long> articleSourceRepository,
    IRepository<Article,string> articleRepository,
    ILogger<ArticleSourceService> logger,
    IArticleSourceHandler articleSourceHandler) : IArticleSourceService
{
    public async Task<string> HandleUpload(IFormFile file, string articleId)
    {
        var articleSource = new ArticleSource();
        var userId = articleRepository.NoTracking
            .Where(a=>a.Id == articleId) 
            .Select(a=>a.AuthorId)
            .First();
        var fileInfo = await articleSourceHandler.StoreSourceFromFormFile(file, userId);
        try
        {
            articleSource.IsHyperLink = false;
            articleSource.Url = null;
            articleSource.Attach(articleId);
            articleSource.FileName = fileInfo.Name;
            await articleSourceRepository.AddAsync(articleSource);
        }
        catch (Exception e)
        {
            logger.LogError(e.Message);
            if(fileInfo.Exists)
                fileInfo.Delete();
            throw;
        }

        return articleSource.FileName;
    }
}