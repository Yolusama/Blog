using Blog.Application.Attributes;
using Blog.Application.Services.Interfaces.ArticleDomain;
using Blog.Domain.ArticleDomain.Entity;
using Blog.Infrastructure.Interfaces;
using Blog.Shared;
using Blog.Shared.Functional;

namespace Blog.Application.Services.ArticleDomain;

[ServiceInjection]
public class ArticleSourceService(
    IRepository<ArticleSource,long> articleSourceRepository,
    IRepository<Article,string> articleRepository,
    ILogger<ArticleSourceService> logger,
    IConfiguration configuration) : IArticleSourceService
{
    public async Task<string> HandleUpload(IFormFile file, string articleId)
    {
        var articleSource = new ArticleSource();
        var guid = RandomGenerator.Guid();
        var extension = Path.GetExtension(file.FileName);
        var userId = articleRepository.NoTracking
            .Where(a=>a.Id == articleId) 
            .Select(a=>a.AuthorId)
            .First();
        var fileName = guid + extension;
        var fileInfo = new FileInfo(Path.Combine(Constants.StaticFilesStorePath,userId.ToString(),fileName));
        if(!fileInfo.Exists)
            fileInfo.Create().Close();
        try
        {
            articleSource.FileName = fileName;
            articleSource.IsHyperLink = false;
            articleSource.Url = null;
            articleSource.Attach(articleId);
            using var stream = fileInfo.OpenWrite();
            await file.CopyToAsync(stream);
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