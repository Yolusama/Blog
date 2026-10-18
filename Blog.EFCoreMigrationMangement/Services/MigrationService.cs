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
    private static readonly string MigrationWorkingFolder = GetMigrationFolder();
    private static readonly string MigrationsFolder = Path.Combine(MigrationWorkingFolder,"Migrations");

    private static string GetMigrationFolder()
    {
        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            throw new InvalidOperationException("迁移只存在于开发调试阶段，生产阶段请使用迁移SQL脚本！");
        const string Management = nameof(Management);
        var currentDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());
        var parentDirectory = currentDirectory.Parent;
        var migrationName = currentDirectory.Name.Substring(0, currentDirectory.Name.Length - Management.Length + 1);
        return Path.Combine(parentDirectory.FullName, migrationName);
    }
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
                .DispatchProcessAsync(DotnetCommand, MigrationWorkingFolder,false,
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
                .DispatchProcessAsync(DotnetCommand, MigrationWorkingFolder,false,
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

    public async Task<ResponseResult> RemoveLatest()
    {
        var migration = await context.Migrations
            .Where(m=>!m.HasApplied)
            .OrderByDescending(m => m.CreateTime).FirstOrDefaultAsync();
        if (migration == null)
            return ResponseResult.Fail("最近无未应用迁移");
        try
        {
            await dispatcher.DispatchProcessAsync(DotnetCommand, MigrationWorkingFolder, false,
                $"ef Migrations remove --context {ToMigrateContext}");
            context.Migrations.Remove(migration);
            await context.SaveChangesAsync();
            return ResponseResult.OK(msg:"操作成功");
        }
        catch (Exception e)
        {
            logger.LogError(e.ToString());
            return ResponseResult.Fail($"程序出现移除：{e.Message}");
        }
    }

    public async Task<ResponseResult> RemoveMigration(string id,string name)
    {
        var migration = await context.Migrations.FirstOrDefaultAsync(x => x.Id == id);
        if(migration == null)
            return ResponseResult.Fail($"数据库迁移 {name} 不存在！");
        try
        {
            context.Migrations.Remove(migration);
            await context.SaveChangesAsync();
            RemoveMigrationFiles(name);
      
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
            return fileNameWithoutExtension.EndsWith(migrationName);
        });
        if(file == null)
            throw new FileNotFoundException($"在目录{MigrationsFolder}下找不到迁移'{migrationName}'相关的文件.");
        return file;
    }
    
    private static void RemoveMigrationFiles(string migrationName)
    {
        var migrationsFolderPath = new DirectoryInfo(MigrationsFolder);
        var files = migrationsFolderPath.GetFiles("*.cs", SearchOption.TopDirectoryOnly).Where(f =>
        {
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(f.Name);
            return fileNameWithoutExtension.EndsWith(migrationName) || 
                   fileNameWithoutExtension.EndsWith($"{migrationName}_Designer");
        });
        foreach (var file in files)
        {
            if(file.Exists)
                file.Delete();
        }
    }
}