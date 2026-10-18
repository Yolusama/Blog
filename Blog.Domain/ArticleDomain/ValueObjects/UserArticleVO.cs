using Blog.Domain.ArticleDomain.Entity;

namespace Blog.Domain.ArticleDomain.ValueObjects;

public class UserArticleVO
{
    /// <summary>
    /// 文章Id
    /// </summary>
    public string ArticleId { get; set; }
    public string Title { get; set; }
    /// <summary>
    /// 作者
    /// </summary>
    public int AuthorId { get; set; }
    /// <summary>
    /// 内容
    /// </summary>
    public string Content { get; set; }
    /// <summary>
    /// 简介摘要
    /// </summary>
    public string Abstract { get; set; }
    /// <summary>
    /// 上传时间
    /// </summary>
    public DateTime? UploadTime { get; set; }
    /// <summary>
    /// 是否为草稿
    /// </summary>
    public bool IsDraft { get; set; }
    public string AuthorName { get; set; }
    public string AuthorAvatar { get; set; }
    public int ViewCount { get; set; }
    public int LikeCount { get; set; }
    public int DislikeCount { get; set; }
    public int CommentCount { get; set; }
}