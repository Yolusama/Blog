using Blog.Shared;

namespace Blog.Domain.ArticleDomain.DTO;

public class ArticleQuery : BasePageQuery
{
    public string Search { get; set; }
}