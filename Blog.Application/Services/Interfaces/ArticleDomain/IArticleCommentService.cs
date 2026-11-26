using Blog.Application.Attributes;
using Blog.Domain.ArticleDomain.DTO;
using Blog.Shared.Enums;

namespace Blog.Application.Services.Interfaces.ArticleDomain;

[ServiceInjection]
public interface IArticleCommentService
{
    public Task<long> WriteComment(ArticleCommentDTO commentDto);
    public Task LikeOrDislikeComment(long commentId,LikeOrDislikeEnum type);
}