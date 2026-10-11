using Blog.Shared;

namespace Blog.Domain.ArticleDomain.Entity;

/// <summary>
/// 文章内资源
/// </summary>
public class ArticleSource : AggregateRoot<long>
{
    /// <summary>
    /// 文件名
    /// </summary>
    public string? FileName { get; set; }
    /// <summary>
    /// 文章Id
    /// </summary>
    public string ArticleId { get; set; }
    /// <summary>
    /// 源地址/其他资源链接
    /// </summary>
    public string Url { get; set; }
    /// <summary>
    /// 是否是超链接
    /// </summary>
    public bool IsHyperLink { get; set; }
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime? CreateTime { get; set; }
}