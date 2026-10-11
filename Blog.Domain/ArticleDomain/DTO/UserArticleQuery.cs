using Blog.Shared;

namespace Blog.Domain.ArticleDomain.DTO;

public class UserArticleQuery : ArticleQuery
{
    public int UserId { get; set; }
    public bool IsDraft { get; set; }
}