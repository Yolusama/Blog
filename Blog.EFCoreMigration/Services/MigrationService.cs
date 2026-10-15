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
            .Count(out var totalCount)
            .OrderByDescending(x => x.CreateTime)
            .Page(pageIndex, pageSize)
            .ToListAsync();
        return ResponseResult.OK(new BasePageQueryResult<Migration>
        {
            Data = data,
            Total = totalCount,
            Page = pageIndex,
            PageSize = pageSize
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
        try
        {
            await dispatcher
                .DispatchProcess(DotnetCommand, true,
                    $"ef migrations add {migrationName} --context {ToMigrateContext}");
            var migrationFile = GetMigrationFileName(migrationName);
            migration.FileName = migrationFile.Name;
            await context.Migrations.AddAsync(migration);
            await context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            logger.LogError(e.ToString());
            return ResponseResult.Fail($"数据库迁移 {migrationName} 添加失败！");
        }
        
        return ResponseResult.OK(msg: $"数据库迁移 {migrationName} 添加成功");
    }

    public async Task<ResponseResult> Migrate(string id,string name)
    {
        var migration = await context.Migrations.FirstOrDefaultAsync(x => x.Id == id);
        try
        {
            if(migration == null)
                throw new FileNotFoundException("迁移不存在");
            await dispatcher
                .DispatchProcess(DotnetCommand, true,
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
       
        return ResponseResult.OK(msg: $"数据库迁移 {name} 更新成功");
    }

    public async Task<ResponseResult> RemoveMigration(string id,string name)
    {
        var migration = await context.Migrations.FirstOrDefaultAsync(x => x.Id == id);
        if(migration == null)
            return ResponseResult.Fail($"数据库迁移 {name} 不存在！");
        try
        {
            await dispatcher.DispatchProcess(DotnetCommand, true, 
                $"ef migrations remove {name} --context {ToMigrateContext}");
            context.Migrations.Remove(migration);
            await context.SaveChangesAsync();
            var migrationFile = GetMigrationFileName(name);
            if (migrationFile.Exists)
                migrationFile.Delete();
      
            return ResponseResult.OK(msg: $"数据库迁移 {name} 删除成功");

        }
        catch (Exception e)
        {
            logger.LogError(e.ToString());
            return ResponseResult.Fail($"数据库迁移 {name} 失败！");
        }
        
    }
    
    private static FileInfo GetMigrationFileName(string migrationName)
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
        return file;
    }
}