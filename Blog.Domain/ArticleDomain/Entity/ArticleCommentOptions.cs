using Blog.Shared;

namespace Blog.Domain.ArticleDomain.Entity;

/// <summary>
/// 文章评论选项信息表
/// </summary>
public class ArticleCommentOptions : AggregateRoot<long>
{
    /// <summary>
    /// 评论Id
    /// </summary>
    public long CommentId { get; private set; }
    /// <summary>
    /// 用户Id
    /// </summary>
    public int UserId { get;private set; }
    /// <summary>
    /// 是否喜欢
    /// </summary>
    public bool Liked { get; set; }
    /// <summary>
    /// 是否讨厌
    /// </summary>
    public bool Disliked { get; set; }
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime? CreateTime { get; private set; } = DateTime.Now;
    /// <summary>
    /// 修改时间
    /// </summary>
    public DateTime? UpdateTime { get;  set; }
    public ArticleCommentOptions()
    {
    }
    
    public ArticleCommentOptions(long commentId, int userId)
    {
        CommentId = commentId;
        UserId = userId;
    }
}