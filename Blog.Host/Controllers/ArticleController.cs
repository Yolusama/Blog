using Blog.Application.Attributes;
using Blog.Application.Services.Interfaces.ArticleDomain;
using Blog.Domain.ArticleDomain.DTO;
using Blog.Domain.ArticleDomain.ValueObjects;
using Blog.Shared;
using Blog.Shared.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Controllers;

[ApplicationService]
public class ArticleController(IArticleService articleService,
    IArticleSourceService articleSourceService,
    IArticleCommentService articleCommentService) : ProjectControllerBase
{
    [HttpGet]
    [Authorize]
    public async Task<BasePageQueryResult<UserArticleVO>> GetUserArticles([FromQuery] UserArticleQuery query)
    {
        return await articleService.GetUserArticles(query);
    }
    
    [HttpGet]
    [Authorize]
    public async Task<BasePageQueryResult<UserArticleVO>> GetRecentArticles([FromQuery] ArticleQuery query)
    {
        return await articleService.GetRecentArticles(query);
    }

    [HttpGet("{articleId}")]
    [Authorize]
    public async Task<ArticleSaveModel> GetArticleContent([FromRoute]string articleId)
    {
        return await articleService.GetArticleContent(articleId);
    }

    [HttpGet("~/api/Article/detail/{articleId}")]
    [Authorize]
    public async Task<ArticleDetailVO> GetArticleDetail([FromRoute]string articleId)
    {
        var user = GetCurrentUser();
        return await articleService.GetArticleDetail(articleId, user.UserId);
    }
    
    [HttpPut]
    [Authorize]
    [UnitOfWork]
    public async Task<ArticleSaveVO> SaveArticle([FromBody] ArticleSaveModel model)
    {
        return await articleService.SaveArticle(model);
    }

    [HttpPost]
    [Authorize]
    public async Task<string> HandleUpload(IFormFile file,[FromForm]string articleId)
    {
        return await articleSourceService.HandleUpload(file, articleId);
    }
    
    [HttpGet]
    [Authorize]
    [UnitOfWork]
    public async Task<ArticleRankingVO> GetArticleRanking()
    {
        return await articleService.GetArticleRanking();
    }
    
    [HttpPatch("{articleId}")]
    [UnitOfWork]
    public async Task ViewArticle([FromRoute]string articleId)
    {
        var user = GetCurrentUser();
        await articleService.ViewArticle(articleId, user.UserId);
    }
    
    [HttpDelete("{articleId}")]
    [UnitOfWork]
    public async Task RemoveArticle([FromRoute]string articleId,[FromQuery]bool trueDelete = false)
    {
        await articleService.RemoveArticle(articleId,trueDelete);
    }
    
    [HttpPut("{articleId}")]
    [UnitOfWork]
    public async Task LikeOrDislikeArticle([FromRoute]string articleId,[FromQuery]LikeOrDislikeEnum type)
    {
        var user = GetCurrentUser();
        await articleService.LikeOrDislikeArticle(articleId, user.UserId, type);
    }
    
    [HttpPost]
    [UnitOfWork]
    public async Task<long> WriteComment([FromBody] ArticleCommentDTO commentDto)
    {
        return await articleCommentService.WriteComment(commentDto);
    }
    
    [HttpPut("{commentId}")]
    [UnitOfWork]
    public async Task LikeOrDislikeComment([FromRoute] long commentId,[FromQuery] LikeOrDislikeEnum type)
    {
        await articleCommentService.LikeOrDislikeComment(commentId, type);
    }
    
    [HttpPut("{articleId}")]
    [UnitOfWork]
    public async Task CollectArticle([FromRoute]string articleId,[FromQuery]bool toCollect)
    {
        var user = GetCurrentUser();
        await articleService.CollectArticle(articleId, user.UserId, toCollect);
    }
    
    [HttpPost("{articleId}")]
    [UnitOfWork]
    public async Task<string> PublishArticle([FromRoute]string articleId)
    {
       return await articleService.PublishArticle(articleId);
    }

    [HttpPatch("{articleId}")]
    public async Task RecoverArticle([FromRoute] string articleId)
    {
        await articleService.RecoverArticle(articleId);
    }
    
    [HttpPost]
    [Authorize]
    public async Task<string> UploadCover(IFormFile file,[FromForm]string articleId)
    {
        return await articleSourceService.UploadArticleCover(file, articleId);
    }

    
}