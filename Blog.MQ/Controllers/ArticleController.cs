using Blog.MQ.Core;
using Blog.MQ.Core.Attributes;
using Blog.MQ.Services.Interfaces;

namespace Blog.MQ.Controllers;

[MQController]
public class ArticleController(IArticleService articleService) : MQControllerBase
{
    [MQAction]
    public async Task Audit(string articleId)
    {
        await articleService.Audit(articleId);
    }
}