using Blog.Domain.ArticleDomain.Entity;
using Blog.Infrastructure.Interfaces;
using Blog.Infrastructure.JobManagement;
using Blog.Shared;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace Blog.JobServices;

[QuartzJob(JobKey = nameof(ClearUnappliedArticleCoverJob),
    JobTriggerId = $"Trigger_{nameof(ClearUnappliedArticleCoverJob)}",
    CronExpression = "0 0/10 * * * ?")]
public class ClearUnappliedArticleCoverJob(IConfiguration configuration,IRepository<Article,string> articleRepository) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        DirectoryInfo userCreationDirectoryBase = new DirectoryInfo(configuration[Constants.StaticFilesStorePath]!);
        if(!userCreationDirectoryBase.Exists)
            return;
        var userCreationDirectories = userCreationDirectoryBase.GetDirectories();
        //dir名称一般为userId
        foreach (var dir in userCreationDirectories)
        {
            foreach (var file in dir.GetFiles().Where(f=>Constants.ImageExtensions.Any(ext=>f.Extension.
                         Equals(ext, StringComparison.OrdinalIgnoreCase))))
            {
                if (int.TryParse(dir.Name, out int userId))
                {
                    if(!await articleRepository.NoTracking.AnyAsync(a=>a.AuthorId == userId
                       && a.Cover == file.Name) && file.Exists)
                        file.Delete();
                }
            }
        }
    }
}