using Blog.Domain.ArticleDomain.Entity;
using Blog.Infrastructure.Interfaces;
using Blog.Infrastructure.JobManagement;
using Blog.Shared;
using Blog.Shared.Enums;
using Blog.Shared.Functional;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace Blog.JobServices;

[QuartzJob(CronExpression = "0 0/10 * * * ?")]
public class ClearUnappliedArticleCoverJob(ILogger<ClearUnappliedArticleCoverJob> logger,
    IConfiguration configuration,IRepository<Article,string> articleRepository) : IJob
{
    private const string JobName = "ClearUnappliedArticleCover";
    private const int MaxClearCount = 100;
    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            logger.LogInformation("{JobName}任务启动，开始清理。。",JobName);
            var userCreationDirectoryBase = new DirectoryInfo(Path.Combine(configuration[Constants.StaticFilesStorePath]!,Constants.UserCreations));
            if(!userCreationDirectoryBase.Exists)
                return;
            var userCreationDirectories = userCreationDirectoryBase.GetDirectories();
            //dir名称一般为userId
            List<string> clearedCovers = [];
                foreach (var dir in userCreationDirectories)
            {
                foreach (var file in dir.GetFiles().Where(f=>Constants.ImageExtensions.Any(ext=>f.Extension.
                             Equals(ext, StringComparison.OrdinalIgnoreCase))))
                {
                    if (int.TryParse(dir.Name, out int userId))
                    {
                        if(file.Exists && !await articleRepository.NoTracking.AnyAsync(a=>a.AuthorId == userId
                                                                           && a.Cover == file.Name))
                        {
                            clearedCovers.Add(file.Name);
                            file.Delete();
                        }
                    }
                }
                if(clearedCovers.Count > MaxClearCount)
                    break;
            }
            logger.LogInformation("{JobName}任务本次清理:{Covers}",JobName,string.Join(';',clearedCovers));
        }
        catch (Exception e)
        {
           logger.LogError(e.ToString());
        }
        
    }
}