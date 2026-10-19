using Blog.Shared;
using Blog.Shared.Functional;

namespace Blog.Domain.ArticleDomain.Entity;

/// <summary>
/// 文章统计选项
/// </summary>
public class ArticleOptions : AggregateRoot<long>
{
    /// <summary>
    /// 文章Id
    /// </summary>
    public string ArticleId { get; set; }
    /// <summary>
    /// 用户Id
    /// </summary>
    public int UserId { get; set; }
    /// <summary>
    /// 是否看过文章
    /// </summary>
    public bool Viewed { get; set; } 
    /// <summary>
    /// 是否点赞过文章
    /// </summary>
    public bool Liked { get; set; } 
    /// <summary>
    /// 是否点踩过文章
    /// </summary>
    public bool DisLiked { get; set; } 
    /// <summary>
    /// 是否评论过文章
    /// </summary>
    public bool HasCommentedOn { get; set; }
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime? CreateTime { get; private set; } = DateTime.Now;
    /// <summary>
    /// 修改时间
    /// </summary>
    public DateTime? UpdateTime { get;  set; }
    public ArticleOptions(){}
    public ArticleOptions(string articleId,int userId)
    {
        Id = RandomGenerator.SnowflakeId();
        ArticleId = articleId;
        UserId = userId;
    }
}