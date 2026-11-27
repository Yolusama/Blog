using Microsoft.AspNetCore.Mvc;
using Blog.EFCoreMigration.Entity;
using Blog.EFCoreMigration.Services.Interfaces;
using Blog.Shared;

namespace Blog.EFCoreMigration.Controllers;

[Route("api/[controller]")]
public class MigrationsController(IMigrationService migrationService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ResponseResult<BasePageQueryResult<Migration>>>> GetAsync(string search,int page,int pageSize)
    {
       return await migrationService.Get(search, page, pageSize);
    }

    [HttpPost]
    public async Task<ActionResult<ResponseResult>> Create([FromBody] MigrationRequest request)
    {
        return await migrationService.AddMigration(request.Name);
    }

    [HttpPut("apply")]
    public async Task<ActionResult<ResponseResult>> Apply([FromBody] MigrationRequest request)
    {
        return await migrationService.Migrate(request.Id, request.Name);
    }

    [HttpDelete(nameof(RemoveLatest))]
    public async Task<ActionResult<ResponseResult>> RemoveLatest()
    {
        return await migrationService.RemoveLatest();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ResponseResult>> DeleteAsync([FromRoute] string id,[FromQuery] string name)
    {
        return await migrationService.RemoveMigration(id, name);
    }

    [HttpGet("generate/{id}")]
    public async Task<ActionResult> GenerateMigrationScript([FromRoute] string id,[FromQuery] string name)
    {
        try
        {
            var (fileName,bytes) = await migrationService.GenerateMigrationScript(id, name);
            return File(bytes, Constants.FileResponseHeader,fileName);
        }
        catch (Exception e)
        {
            // 生成失败时返回可读信息，前端会从 ArrayBuffer 里解析 message 直接提示
            return BadRequest(ResponseResult.Fail(e is FileNotFoundException
                ? e.Message
                : $"生成迁移脚本 {name} 失败：{e.Message}"));
        }
    }
    
    [HttpGet("fullyGenerate")]
    public async Task<ActionResult> GenerateMigrationScript()
    {
        try
        {
            var (fileName,bytes) = await migrationService.GenerateMigrationScript();
            return File(bytes, Constants.FileResponseHeader, fileName);
        }
        catch (Exception e)
        {
            return BadRequest(ResponseResult.Fail($"生成全量迁移脚本失败：{e.Message}"));
        }
    }
}

public struct MigrationRequest
{
    public string Id { get; set; }
    public string Name { get; set; }
}
