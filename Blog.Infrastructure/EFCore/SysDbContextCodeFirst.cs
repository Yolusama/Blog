using System.Reflection;
using Blog.Domain.ArticleDomain.Entity;
using Blog.Domain.UserDomain.Entity;
using Blog.Shared;
using Microsoft.EntityFrameworkCore;

namespace Blog.Infrastructure;

public partial class SysDbContext
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        #region 用户表迁移

        builder.Entity<User>().ToTable(nameof(User), t => t.HasComment("用户表"));
        builder.Entity<User>().HasKey(u => u.Id);
        builder.Entity<User>().Property(u => u.Id)
            .UseMySqlIdentityColumn()
            .HasComment("用户Id")
            .HasColumnType("int");
        builder.Entity<User>().Property(u => u.Email)
            .HasMaxLength(100)
            .IsRequired()
            .HasComment("邮箱");
        builder.Entity<User>().HasIndex(u => u.Email).IsUnique();
        builder.Entity<User>().Property(u => u.Password)
            .IsRequired()
            .HasMaxLength(256)
            .HasComment("密码");
        builder.Entity<User>().Property(u => u.Nickname)
            .IsRequired()
            .HasMaxLength(64)
            .HasComment("昵称");
        builder.Entity<User>().Property(u => u.Avatar)
            .HasMaxLength(50)
            .HasComment("头像");
        builder.Entity<User>().Property(u => u.Gender)
            .HasDefaultValue(0)
            .HasComment("性别");
        builder.Entity<User>().Property(u => u.SimpleIntroduction)
            .IsRequired(false)
            .HasMaxLength(1000)
            .HasComment("简介");
        builder.Entity<User>().Property(u => u.BirthDay)
            .HasColumnType("date")
            .HasComment("生日");
        builder.Entity<User>().Property(u => u.Region)
            .IsRequired(false)
            .HasMaxLength(128)
            .HasComment("地区");
        builder.Entity<User>().Property(u => u.CreateTime)
            .HasColumnType("datetime")
            .HasComment("创建时间");
        builder.Entity<User>().Property(u => u.LastLoginTime)
            .HasColumnType("datetime")
            .HasComment("上次登录时间");

        #endregion

        #region 文章表迁移

        builder.Entity<Article>().ToTable(nameof(Article),
            t => t.HasComment("文章表"));

        builder.Entity<Article>()
            .HasKey(a => a.Id);
        builder.Entity<Article>()
            .Property(a => a.Id)
            .HasMaxLength(64)
            .HasComment("文章Id");
        builder.Entity<Article>()
            .Property(a => a.Title)
            .HasMaxLength(256)
            .HasComment("标题");
        builder.Entity<Article>()
            .Property(a => a.Content)
            .HasMaxLength(100)
            .IsRequired(false)
            .HasComment("文章最终内容指向源");
        builder.Entity<Article>()
            .Property(a => a.Abstract)
            .IsRequired(false)
            .HasMaxLength(256);
        builder.Entity<Article>()
            .Property(a => a.IsDraft)
            .HasComment("是否为草稿")
            .HasColumnType("tinyint(1)");
        builder.Entity<Article>()
            .Property(a => a.Abstract)
            .HasMaxLength(128)
            .IsRequired(false)
            .HasComment("摘要");
        builder.Entity<Article>()
            .Property(a => a.CreateTime)
            .HasComment("创建时间")
            .HasColumnType("datetime");
        builder.Entity<Article>()
            .Property(a => a.UpdateTime)
            .HasComment("更新时间")
            .HasColumnType("datetime");
        builder.Entity<Article>()
            .Property(a => a.UploadTime)
            .HasComment("正式上传时间")
            .HasColumnType("datetime");
        builder.Entity<Article>()
            .Property(a => a.AuthorId)
            .HasComment("作者Id")
            .HasColumnType("int");
        builder.Entity<Article>()
            .Property(x => x.ViewCount)
            .IsRequired()
            .HasComment("查看数");
        builder.Entity<Article>().Property(x => x.LikeCount)
            .HasComment("点赞数");
        builder.Entity<Article>().Property(x => x.DislikeCount)
            .IsRequired()
            .HasComment("点踩数");
        builder.Entity<Article>().Property(x => x.Cover)
            .IsRequired()
            .HasMaxLength(100)
            .HasComment("文章封面");
        
        builder.Entity<Article>()
            .HasIndex(e => e.AuthorId)
            .HasDatabaseName("IDX_Article_AuthorId");
        builder.Entity<Article>()
            .HasIndex(e => e.CreateTime)
            .HasDatabaseName("IDX_Article_CreateTime");
        builder.Entity<Article>()
            .HasIndex(e => e.IsDraft)
            .HasDatabaseName("IDX_Article_IsDraft");

        #endregion

        #region 文章选项信息表迁移

        builder.Entity<ArticleOptions>().ToTable(nameof(ArticleOptions), t =>
            t.HasComment("文章选项信息表"));

        builder.Entity<ArticleOptions>()
            .HasKey(x => x.Id);
        builder.Entity<ArticleOptions>()
            .Property(x => x.Id)
            .HasColumnType("bigint")
            .HasComment("文章选项信息Id");
        builder.Entity<ArticleOptions>()
            .Property(x => x.ArticleId)
            .IsRequired()
            .HasMaxLength(64)
            .HasComment("文章Id");
        builder.Entity<ArticleOptions>()
            .Property(x => x.UserId)
            .IsRequired()
            .HasMaxLength(64)
            .HasComment("用户Id");
        builder.Entity<ArticleOptions>()
            .Property(x => x.Liked)
            .HasColumnType("tinyint(1)")
            .HasComment("是否点赞");
         builder.Entity<ArticleOptions>()
             .Property(x => x.DisLiked)
             .HasColumnType("tinyint(1)")
             .HasComment("是否点踩");
         builder.Entity<ArticleOptions>()
             .Property(x => x.Viewed)
             .HasColumnType("tinyint(1)")
             .HasComment("是否查看过");
         builder.Entity<ArticleOptions>()
             .Property(x => x.CreateTime)
             .HasComment("创建时间");
         builder.Entity<ArticleOptions>()
             .Property(x => x.UpdateTime)
             .HasComment("更新时间");
         
         builder.Entity<ArticleOptions>()
             .HasIndex(x => x.CreateTime)
             .HasDatabaseName("Index_ArticleOptions_CreateTime");
        builder.Entity<ArticleOptions>()
            .HasIndex(x => new {x.ArticleId, x.UserId})
            .HasDatabaseName("Index_ArticleOptions_ArticleId_UserId");

        #endregion

        #region 文字评论表迁移

        builder.Entity<ArticleComment>()
            .ToTable(nameof(ArticleComment), t => t.HasComment("文章评论表"));

        builder.Entity<ArticleComment>()
            .HasKey(x => x.Id);
        builder.Entity<ArticleComment>()
            .Property(x => x.Id)
            .UseMySqlIdentityColumn()
            .HasColumnType("bigint")
            .HasComment("文章评论Id");
        builder.Entity<ArticleComment>().Property(x => x.Content)
            .IsRequired()
            .HasMaxLength(2000)
            .HasComment("内容");
        builder.Entity<ArticleComment>()
            .Property(x => x.CreateTime) 
            .HasComment("创建时间");
        builder.Entity<ArticleComment>().Property(x => x.LikeCount)
            .IsRequired()
            .HasComment("喜欢数");
        builder.Entity<ArticleComment>()
            .Property(x => x.DislikeCount)
            .IsRequired()
            .HasComment("讨厌数");
        builder.Entity<ArticleComment>()
            .Property(x => x.ArticleId)
            .IsRequired()
            .HasMaxLength(64)
            .HasComment("文章Id");
        builder.Entity<ArticleComment>().Property(x => x.RootId)
            .IsRequired()
            .HasDefaultValue(Constants.EOF)
            .HasComment("根ID（指向根评论的 Id，根评论为 -1），防止嵌套过深往往只有一个根评论");
        builder.Entity<ArticleComment>().Property(x => x.UserId)
            .IsRequired()
            .HasComment("发起者");
        builder.Entity<ArticleComment>().Property(x => x.CommentToId)
            .IsRequired(false)
            .HasColumnType("bigint")
            .HasComment("除根评论外的评论Id，根评论此项为空");
        builder.Entity<ArticleComment>().Property(x => x.PostTime)
            .IsRequired(false)
            .HasComment("发表时间");

        builder.Entity<ArticleComment>()
            .HasIndex(x => x.RootId)
            .HasDatabaseName("IX_ArticleComment_RootId");
        builder.Entity<ArticleComment>()
            .HasIndex(x => x.ArticleId)
            .HasDatabaseName("IX_ArticleComment_Article");
        builder.Entity<ArticleComment>()
            .HasIndex(x => x.PostTime)
            .HasDatabaseName("IX_ArticleComment_PostTime");

        #endregion

        #region 文章资源表迁移

        builder.Entity<ArticleSource>().ToTable(nameof(ArticleSource),
            t => t.HasComment("文章内资源表"));
        
        builder.Entity<ArticleSource>()
            .HasKey(x => x.Id);

        builder.Entity<ArticleSource>()
            .Property(x => x.Id)
            .UseMySqlIdentityColumn()
            .HasColumnType("bigint");
        builder.Entity<ArticleSource>().Property(x => x.FileName)
            .HasMaxLength(255)
            .IsRequired(false) // string? -> 可空
            .HasComment("文件名");
        builder.Entity<ArticleSource>().Property(x => x.ArticleId)
            .IsRequired()
            .HasMaxLength(64) // 与 Article 主键长度一致
            .HasComment("文章Id");
        builder.Entity<ArticleSource>().Property(x => x.Url)
            .HasMaxLength(2000) // 按需调整；超长可去掉这行用 nvarchar(max)
            .HasComment("源地址/其他资源链接");
        builder.Entity<ArticleSource>().Property(x => x.IsHyperLink)
            .HasDefaultValue(false)
            .IsRequired()
            .HasComment("是否是超链接");
        builder.Entity<ArticleSource>()
            .Property(x => x.CreateTime) // SQL Server；MySQL 用 CURRENT_TIMESTAMP
            .HasComment("创建时间");
        builder.Entity<ArticleSource>().HasIndex(x => x.ArticleId)
            .HasDatabaseName("IX_ArticleSource_ArticleId");
        builder.Entity<ArticleSource>()
            .HasIndex(x => x.CreateTime)
            .HasDatabaseName("IX_ArticleSourceCreateTime");
        #endregion

        #region 文章评论选项信息表迁移
        builder.Entity<ArticleCommentOptions>()
            .ToTable(nameof(ArticleCommentOptions), t => t.HasComment("文章评论选项信息表"));

        builder.Entity<ArticleCommentOptions>(configure =>
        {
               configure.HasKey(x => x.Id);
               
               configure.Property(x => x.Id)
                     .HasColumnType("bigint")
                     .UseMySqlIdentityColumn()
                     .HasComment("文章评论选项信息Id");
               configure.Property(x => x.CommentId)
                     .IsRequired()
                     .HasColumnType("bigint")
                     .HasComment("文章评论Id");
               configure.Property(x => x.UserId)
                     .IsRequired()
                     .HasColumnType("int")
                     .HasComment("用户Id");
               configure.Property(x => x.Liked)
                     .IsRequired()
                     .HasColumnType("tinyint(1)")
                     .HasComment("是否点赞");
               configure.Property(x => x.Disliked)
                   .IsRequired()
                   .HasColumnType("tinyint(1)")
                     .HasComment("是否点踩");
               configure.Property(x => x.CreateTime)
                   .HasColumnType("datetime")
                   .HasComment("创建时间");
               configure.Property(x => x.UpdateTime)
                     .HasColumnType("datetime")
                     .HasComment("修改时间");
               
               configure.HasIndex(x => x.UserId)
                   .HasDatabaseName("IX_ArticleCommentOptions_UserId");
               configure.HasIndex(x => x.CommentId)
                   .HasDatabaseName("IX_ArticleCommentOptions_CommentId");
               configure.HasIndex(x => x.CreateTime)
                   .HasDatabaseName("Index_ArticleCommentOptions_CreateTime");
        });

        #endregion

        #region API审计日志表迁移

        builder.Entity<ProjectAuditLog>()
            .ToTable(nameof(ProjectAuditLog), t => t.HasComment("API审计日志表"));

        builder.Entity<ProjectAuditLog>(configure =>
        {
            configure.HasKey(x => x.Id);

            configure.Property(x => x.Id)
                .UseMySqlIdentityColumn()
                .HasColumnType("bigint")
                .HasComment("日志Id");
            configure.Property(x => x.ActionName)
                .HasMaxLength(128)
                .IsRequired()
                .HasComment("Action方法名");
            configure.Property(x => x.ServiceName)
                .HasMaxLength(128)
                .IsRequired()
                .HasComment("服务/控制器名");
            configure.Property(x => x.ActionType)
                .HasMaxLength(16)
                .IsRequired()
                .HasComment("请求方式");
            configure.Property(x => x.ActionDesc)
                .HasMaxLength(200)
                .IsRequired()
                .HasComment("Action描述内容");
            configure.Property(x => x.Route)
                .HasMaxLength(200)
                .IsRequired()
                .HasComment("请求路由");
            configure.Property(x => x.RemoteIpAddr)
                .HasMaxLength(64)
                .IsRequired(false)
                .HasComment("远程请求IP");
            configure.Property(x => x.ParameterJson)
                .HasColumnType("json")
                .IsRequired(false)
                .HasComment("请求参数Json");
            configure.Property(x => x.Exception)
                .HasColumnType("longtext")
                .IsRequired(false)
                .HasComment("异常信息");
            configure.Property(x => x.ExecutionTime)
                .HasColumnType("datetime")
                .HasComment("执行时间");
            configure.Property(x => x.ExecutionDuration)
                .HasComment("执行耗时（毫秒）");
            configure.Property(x => x.ClientInfo)
                .HasMaxLength(200)
                .HasComment("客户端请求信息（请求头User-Agent)");

            configure.HasIndex(x => x.ExecutionTime)
                .HasDatabaseName("IX_ProjectAuditLog_ExecutionTime");
            configure.HasIndex(x => x.ActionName)
                .HasDatabaseName("IX_ProjectAuditLog_ActionName");
        });

        #endregion
    }
}