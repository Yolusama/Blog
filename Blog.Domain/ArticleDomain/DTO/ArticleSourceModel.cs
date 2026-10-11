namespace Blog.Domain.ArticleDomain.DTO;

public class ArticleSourceModel
{
    public long SourceId { get; set; }
    public string? FileName { get; set; }
    public string ArticleId { get; set; }
    public string Url { get; set; }
    public bool IsHyperLink { get; set; }
}