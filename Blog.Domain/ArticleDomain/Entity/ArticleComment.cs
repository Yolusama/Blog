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
    /// 根ID，防止嵌套过深往往只有一个根评论
    /// </summary>
    public long RootId { get; private set; }
    /// <summary>
    /// 文章Id
    /// </summary>
    public string ArticleId { get; private set; }
    /// <summary>
    /// 发起者
    /// </summary>
    public int UserId { get; set; }
    /// <summary>
    /// 除根评论外的评论Id，根评论此项为空
    /// </summary>
    public long? CommentToId { get; set; }
    /// <summary>
    /// 发表时间
    /// </summary>
    public DateTime? PostTime { get;  set; }

    public void SetRootId(long rootId)
    {
        if (rootId == Constants.EOF)
           throw new ArgumentException("非根ID值必须为其评论的对应根Id值");
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
    
    
    public ArticleComment(){}
    
    public ArticleComment(string articleId)
    {
        ArticleId = articleId;
        RootId = Constants.EOF;
        LikeCount = 0;
        DislikeCount = 0;
    }
}