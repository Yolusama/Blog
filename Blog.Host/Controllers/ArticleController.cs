using Blog.Application.Attributes;
using Blog.Application.Services.Interfaces.ArticleDomain;
using Blog.Domain.ArticleDomain.DTO;
using Blog.Domain.ArticleDomain.ValueObjects;
using Blog.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Controllers;

[ApplicationService]
public class ArticleController(IArticleService articleService,IArticleSourceService articleSourceService) : ProjectControllerBase
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
    
    [HttpPut]
    [Authorize]
    public async Task<string> SaveArticle([FromBody] ArticleSaveModel model)
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
    public async Task<ArticleRankingVO> GetArticleRanking()
    {
        return await articleService.GetArticleRanking();
    }
}