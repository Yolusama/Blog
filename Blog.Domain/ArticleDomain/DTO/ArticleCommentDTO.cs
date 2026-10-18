namespace Blog.Domain.ArticleDomain.DTO;

public class ArticleCommentDTO
{
    public string ArticleId { get; set; }
    public int UserId { get; set; }
    public string Content { get; set; }
    public long RootId { get; set; }
    public long? CommentToId { get; set; }
}