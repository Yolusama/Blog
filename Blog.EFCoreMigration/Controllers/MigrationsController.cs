using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Blog.EFCoreMigration.Entity;
using Blog.EFCoreMigration.Services.Interfaces;
using Blog.Shared;

namespace Blog.EFCoreMigration.Controllers;

[ApiController]
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

    [HttpPut]
    public async Task<ActionResult<ResponseResult>> Update([FromBody] MigrationRequest request)
    {
        return await migrationService.Migrate(request.Id, request.Name);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ResponseResult>> DeleteAsync([FromRoute] string id,[FromQuery] string name)
    {
        return await migrationService.RemoveMigration(id, name);
    }
}

public struct MigrationRequest
{
    public string Id { get; set; }
    public string Name { get; set; }
}
