namespace Blog.Domain.ArticleDomain.DTO;

public class ArticleSaveModel
{
    public string? ArticleId { get; set; }
    public string Cover { get; set; }
    public string CreationDeclaration {get; set;}
    public string? Title { get; set; }
    public string? Content { get; set; }
    public string? ArticleTrueContent { get; set; }
    public string? ArticleOriginalContent { get; set; }
    public string? Abstract { get; set; }
    public int UserId  { get; set; }
    public bool IsDraft { get; set; }
}