using Microsoft.AspNetCore.Http;

namespace Blog.Domain.ArticleDomain.Services.Interfaces;

public interface IArticleSourceHandler
{
    public Task StoreArticleContent(string fileName,int userId,string content);
    public Task<string> ReadArticleContent(string fileName,int userId);
    public Task RemoveArticleFile(string fileName,int userId);
    public Task RemoveSource(string fileName,int userId);
    public Task<FileInfo> StoreSourceFromFormFile(IFormFile file,int userId);
}