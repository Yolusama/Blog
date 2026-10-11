using Blog.Shared;

namespace Blog.Domain.ArticleDomain.Entity;

/// <summary>
/// 文章评论
/// </summary>
public class ArticleComment : AggregateRoot<long>
{
    /// <summary>
    /// 内容
    /// </summary>
    public string Content { get; set; }
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime? CreateTime { get;private set; } = DateTime.Now;
    /// <summary>
    /// 喜欢数
    /// </summary>
    public int LikeCount { get;private set; }
    /// <summary>
    /// 讨厌数
    /// </summary>
    public int DislikeCount { get;private set; }
    /// <summary>
    /// 根ID
    /// </summary>
    public long RootId { get; private set; }
    /// <summary>
    /// 为满足Id
    /// </summary>
    public string ArticleId { get; private set; }

    public void SetRootId(long rootId)
    {
        if (rootId == Constants.EOF)
           throw new ArgumentException($"非根ID值必须为每个评论的Id值");
        RootId = rootId;
    }
    
    public void Like()
    {
        LikeCount++;
        if(DislikeCount > 0)
          DislikeCount--;
    }

    public void Dislike()
    {
        DislikeCount++;
        if(LikeCount > 0)
          LikeCount--;
    }
    
    public ArticleComment(string articleId)
    {
        ArticleId = articleId;
        RootId = Constants.EOF;
        LikeCount = 0;
        DislikeCount = 0;
    }
}