using Blog.Application.Attributes;
using Blog.Application.Services.Interfaces.ArticleDomain;
using Blog.Domain.ArticleDomain.DTO;
using Blog.Domain.ArticleDomain.ValueObjects;
using Blog.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Controllers;

[ApplicationService]
public class ArticleController(IArticleService articleService) : ProjectControllerBase
{
    [HttpGet]
    [Authorize]
    public async Task<BasePageQueryResult<UserArticleVO>> GetUserArticles([FromQuery] UserArticleQuery query)
    {
        return await articleService.GetUserArticles(query);
    }
    
    [HttpPut]
    [Authorize]
    public async Task<string> SaveArticle([FromBody] ArticleSaveModel  model)
    {
        return await articleService.SaveArticle(model);
    }
}