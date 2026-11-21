using Blog.MQ.Functional;
using Blog.MQ.Model.Entity;
using Blog.MQ.Model.Enum;
using Blog.MQ.Services.Interfaces;

namespace Blog.MQ.Services;

public class ArticleService(IFreeSql orm,ILogger<ArticleService> logger) : IArticleService
{
    public async Task Audit(string articleId)
    {
      var execResult = await orm.TransactionAsync(async worker =>
        {
          await worker.Orm.Update<Article>()
              .Set(a=>a.PublishStatus,ArticlePublishStatus.Published.IntValue())
              .Set(a=>a.PublishTime,DateTime.Now)
              .Set(a=>a.UpdateTime,DateTime.Now)
              .Where(a=>a.PublishStatus == ArticlePublishStatus.Auditing.IntValue() 
              && a.Id == articleId && !a.IsDeleted && !a.IsDraft)
              .ExecuteAffrowsAsync();
        },ex=>logger.LogError("出现异常：{ex}",ex));
      if(execResult)
          logger.LogInformation("已审核文章:{articleId}",articleId);
    }
}