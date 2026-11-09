using System.Text;
using Blog.Domain.ArticleDomain.Services.Interfaces;
using Blog.Shared;
using Blog.Shared.Exceptions;
using Blog.Shared.Functional;

namespace Blog.Application.Services.ArticleDomain;

public class ArticleSourceHandler(IConfiguration configuration) : IArticleSourceHandler
{
    private const string ContentBase = "Creations";
    private const string Source = nameof(Source);
    public async Task<string> StoreArticleContent(string fileName,int userId, string content)
    {
        var path = Path.Combine(configuration[Constants.StaticFilesStorePath],ContentBase, userId.ToString(),fileName);
        var fileInfo = new FileInfo(path);
        fileInfo.CreateIfNotExists();
        using var stream = fileInfo.OpenWrite();
        var bytes = Encoding.UTF8.GetBytes(content);
        await stream.WriteAsync(bytes);
        return fileInfo.Name;
    }

    public async Task<string> ReadArticleContent(string fileName, int userId)
    {
        var path = Path.Combine(configuration[Constants.StaticFilesStorePath],ContentBase,userId.ToString(), fileName);
        var fileInfo = new FileInfo(path);
        if(!fileInfo.Exists)
            throw new FileNotFoundException("File not found");
        using var stream = fileInfo.OpenRead();
        var bytes = new byte[stream.Length];
        int bytesRead = await stream.ReadAsync(bytes);
        return Encoding.UTF8.GetString(bytes, 0, bytesRead);
    }

    public async Task RemoveArticleFile(string fileName, int userId)
    {
        await Task.Run(() =>
        {
            var path = Path.Combine(configuration[Constants.StaticFilesStorePath], ContentBase, userId.ToString(),
                fileName);
            var fileInfo = new FileInfo(path);
            if (fileInfo.Exists)
                fileInfo.Delete();
        });
    }

    public async Task RemoveSource(string fileName, int userId)
    {
        await Task.Run(() =>
        {
            var path = Path.Combine(configuration[Constants.StaticFilesStorePath], Source,userId.ToString(),
                fileName);
            var fileInfo = new FileInfo(path);
            if (fileInfo.Exists)
                fileInfo.Delete();
        });
    }

    public async Task<FileInfo> StoreSourceFromFormFile(IFormFile file, int userId)
    {
        try
        {
            var guid = RandomGenerator.Guid();
            var extension = Path.GetExtension(file.FileName);
            var fileName = guid + extension;
            var fileInfo = new FileInfo(Path.Combine(configuration[Constants.StaticFilesStorePath],Source,userId.ToString(),fileName));
            fileInfo.CreateIfNotExists();
            using var stream = fileInfo.OpenWrite();
            await file.CopyToAsync(stream);
            return fileInfo;
        }
        catch (Exception e)
        {
            throw new ServiceException(e.Message);
        }
    }
}