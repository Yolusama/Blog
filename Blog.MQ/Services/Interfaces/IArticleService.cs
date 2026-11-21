namespace Blog.MQ.Services.Interfaces;

public interface IArticleService
{
    /// <summary>
    /// 暂时使用MQ直接通过审核
    /// </summary>
    public Task Audit(string articleId);
}