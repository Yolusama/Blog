namespace Blog.Domain.ArticleDomain.ValueObjects;

public class ArticleSaveVO
{
    public string ArticleId { get; set; }
    /// <summary>
    /// 文章保存源
    /// </summary>
    public string Content { get; set; }
}