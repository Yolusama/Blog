using Blog.Shared;
using Blog.Shared.Functional;

namespace Blog.Domain.ArticleDomain.Entity;

/// <summary>
/// 文章
/// </summary>
public class Article : AggregateRoot<string>
{
    public Article()
    {
        ViewCount = 0;
        LikeCount = 0;
        DislikeCount = 0;
    }
    public Article(bool isDraft) : this()
    {
        Id = RandomGenerator.Guid();
        IsDraft = isDraft;
    }
    /// <summary>
    /// 标题
    /// </summary>
    public string Title { get; set; }
    /// <summary>
    /// 作者
    /// </summary>
    public int AuthorId { get; set; }
    /// <summary>
    /// 内容(储存源，md格式模式保存）
    /// </summary>
    public string Content { get; set; }
    /// <summary>
    /// 简介摘要
    /// </summary>
    public string Abstract { get; set; }
    /// <summary>
    /// 上传时间
    /// </summary>
    public DateTime? UploadTime { get;private set;}
    /// <summary>
    /// 是否为草稿
    /// </summary>
    public bool IsDraft { get; set; }
    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime? UpdateTime { get; set; }
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime? CreateTime { get; private set; } = DateTime.Now;
    /// <summary>
    /// 形式删除-进入回收站
    /// </summary>
    public bool IsDeleted { get; set; }
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

    public void WhenUpload()
    {
        IsDraft = false;
        UploadTime = DateTime.Now;
    }
}