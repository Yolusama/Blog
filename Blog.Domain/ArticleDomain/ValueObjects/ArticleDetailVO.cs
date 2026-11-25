namespace Blog.Domain.ArticleDomain.ValueObjects;

/// <summary>
/// 文章详情，包含正文与作者、互动统计信息
/// </summary>
public class ArticleDetailVO
{
    public string ArticleId { get; set; }
    public string Title { get; set; }
    /// <summary>
    /// 简介摘要
    /// </summary>
    public string Abstract { get; set; }
    /// <summary>
    /// 文章封面
    /// </summary>
    public string Cover { get; set; }
    /// <summary>
    /// 正文 Html
    /// </summary>
    public string Content { get; set; }
    /// <summary>
    /// 创作声明
    /// </summary>
    public string? CreationDeclaration { get; set; }
    public int AuthorId { get; set; }
    public string AuthorName { get; set; }
    public string AuthorAvatar { get; set; }
    public int ViewCount { get; set; }
    public int LikeCount { get; set; }
    public int DislikeCount { get; set; }
    public int CollectCount { get; set; }
    public int CommentCount { get; set; }
    public DateTime? CreateTime { get; set; }
    public DateTime? PublishTime { get; set; }
    /// <summary>
    /// 当前用户是否点赞
    /// </summary>
    public bool Liked { get; set; }
    /// <summary>
    /// 当前用户是否点踩
    /// </summary>
    public bool Disliked { get; set; }
    /// <summary>
    /// 当前用户是否已收藏
    /// </summary>
    public bool Collected { get; set; }
}
