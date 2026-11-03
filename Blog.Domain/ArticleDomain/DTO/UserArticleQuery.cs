using Blog.Shared;

namespace Blog.Domain.ArticleDomain.DTO;

public class UserArticleQuery : ArticleQuery
{
    public int UserId { get; set; }
    public bool? IsDraft { get; set; }
    public int? PublishStatus { get; set; }
    public bool? IsDeleted { get; set; } = false;
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
}