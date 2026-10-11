using Blog.Shared;
using Blog.Shared.Functional;

namespace Blog.Domain.ArticleDomain.Entity;

/// <summary>
/// 文章统计选项
/// </summary>
public class ArticleOptions : AggregateRoot<long>
{
    /// <summary>
    /// 查看数
    /// </summary>
    public int ViewCount { get;private set; }
    /// <summary>
    /// 点赞数
    /// </summary>
    public int LikeCount { get;private set; }
    /// <summary>
    /// 点踩数
    /// </summary>
    public int DislikeCount { get;private set; }
    /// <summary>
    /// 文章Id
    /// </summary>
    public string ArticleId { get;private set; }

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

    public void View()
    {
        ViewCount++;
    }
    
    public ArticleOptions(string articleId)
    {
        Id = RandomGenerator.SnowflakeId();
        ArticleId = articleId;
        ViewCount = 0;
        LikeCount = 0;
        DislikeCount = 0;
    }
}