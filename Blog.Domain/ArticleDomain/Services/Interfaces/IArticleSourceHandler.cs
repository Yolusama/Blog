using Microsoft.AspNetCore.Http;

namespace Blog.Domain.ArticleDomain.Services.Interfaces;

public interface IArticleSourceHandler
{
    public Task<string> StoreArticleContent(string fileName,int userId,string content);
    public Task<string> ReadArticleContent(string fileName,int userId);
    public Task RemoveArticleFile(string fileName,int userId);
    public Task RemoveSource(string fileName,int userId);
    public Task<FileInfo> StoreSourceFromFormFile(IFormFile file,int userId);
    /// <summary>
    /// 判断资源文件是否存在
    /// </summary>
    /// <param name="fileName">文件名</param>
    /// <param name="userId">用户Id</param>
    public bool SourceExists(string fileName,int userId);
}