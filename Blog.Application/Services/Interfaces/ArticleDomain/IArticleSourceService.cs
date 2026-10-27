using Blog.Application.Attributes;

namespace Blog.Application.Services.Interfaces.ArticleDomain;

[ServiceInjection]
public interface IArticleSourceService
{
    public Task<string> HandleUpload(IFormFile file,string articleId);
}