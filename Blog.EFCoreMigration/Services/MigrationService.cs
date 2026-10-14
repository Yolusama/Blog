using Blog.EFCoreMigration.Entity;
using Blog.EFCoreMigration.Services.Interfaces;
using Blog.Infrastructure;
using Blog.Infrastructure.Expansions;
using Blog.Shared;
using Microsoft.EntityFrameworkCore;

namespace Blog.EFCoreMigration.Services;

public class MigrationService(MigrationDbContext context,
    IProcessDispatcher dispatcher,
    ILogger<MigrationService> logger) : IMigrationService
{
    private const string DotnetCommand = "dotnet";
    private const string ToMigrateContext = nameof(SysDbContext);
    private const string MigrationsFolder = "Migrations";
    
    public async Task<ResponseResult<BasePageQueryResult<Migration>>> Get(string searchKey, int pageIndex, int pageSize)
    {
        var data = await context.Migrations
            .WhereIf(!string.IsNullOrWhiteSpace(searchKey), x => x.Name.Contains(searchKey))
            .OrderByDescending(x => x.CreateTime)
            .Page(pageIndex, pageSize)
            .Count(out var totalCount)
            .ToListAsync();
        return ResponseResult.OK(new BasePageQueryResult<Migration>
        {
            Data = data,
            Total = totalCount
        });
    }

    public async Task<ResponseResult> AddMigration(string migrationName)
    {
        if (await context.Migrations.AnyAsync(x => x.Name == migrationName))
            return ResponseResult.Fail($"数据库迁移 {migrationName} 已存在，请使用其他名称。");
        var migration = new Migration
        {
            Id = Guid.NewGuid().ToString(),
            Name = migrationName,
            CreateTime = DateTime.Now,
            HasApplied = false
        };
        var name = migration.Name;
        try
        {
            await dispatcher
                .DispatchProcess(DotnetCommand, 
                    $"ef migrations add {name} --context {ToMigrateContext}");
            migration.FileName = GetMigrationFileName(name);
            await context.Migrations.AddAsync(migration);
        }
        catch (Exception e)
        {
            logger.LogError(e.ToString());
            await context.SaveChangesAsync();
        }
        
        return ResponseResult.OK();
    }

    public async Task<ResponseResult> Migrate(string id,string name)
    {
        var migration = await context.Migrations.FirstOrDefaultAsync(x => x.Id == id);
        try
        {
            if(migration == null)
                throw new FileNotFoundException("迁移不存在");
            await dispatcher
                .DispatchProcess(DotnetCommand, 
                    $"ef database update {name} --context {ToMigrateContext}");
            migration.HasApplied = true;
            migration.ApplyTime = DateTime.Now;
            await context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            logger.LogError(e.ToString());
            return ResponseResult.Fail($"数据库迁移 {name} 失败！");
        }
       
        return ResponseResult.OK(msg:"操作成功");
    }

    public async Task<ResponseResult> RemoveMigration(string id,string name)
    {
        var migration = await context.Migrations.FirstOrDefaultAsync(x => x.Id == id);
        if(migration == null)
            return ResponseResult.Fail($"数据库迁移 {name} 不存在！");
        try
        {
            await dispatcher.DispatchProcess(DotnetCommand, 
                $"ef migrations remove {name} --context {ToMigrateContext}");
            context.Migrations.Remove(migration);
            await context.SaveChangesAsync();
            return ResponseResult.OK();
        }
        catch (Exception e)
        {
            logger.LogError(e.ToString());
            return ResponseResult.Fail($"数据库迁移 {name} 失败！");
        }
        
    }
    
    private static string GetMigrationFileName(string migrationName)
    {
        var migrationsFolderPath = new DirectoryInfo(MigrationsFolder);
        var files = migrationsFolderPath.GetFiles("*.cs", SearchOption.TopDirectoryOnly);
        var file = files.FirstOrDefault(f =>
        {
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(f.Name);
            return fileNameWithoutExtension.Equals(migrationName);
        });
        if(file == null)
            throw new FileNotFoundException($"Migration file '{migrationName}.cs' not found in the '{MigrationsFolder}' folder.");
        return file.Name;
    }
}