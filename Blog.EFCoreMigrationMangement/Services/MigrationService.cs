using Blog.EFCoreMigration.Entity;
using Blog.EFCoreMigration.Services.Interfaces;
using Blog.Infrastructure;
using Blog.Infrastructure.Expansions;
using Blog.Shared;
using Blog.Shared.Exceptions;
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
            if (migration.HasApplied)
                return ResponseResult.Fail("迁移已应用！");
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

    public async Task<(string,byte[])> GenerateMigrationScript(string id, string name)
    {
        // 按 EF 迁移链（迁移 Id 中的时间戳前缀）解析目标迁移，支持传入完整 Id 或仅名称
        var migrationIds = GetOrderedMigrationIds();
        var targetIndex = migrationIds.FindIndex(m =>
            string.Equals(m, name, StringComparison.Ordinal) ||
            m.EndsWith($"_{name}", StringComparison.Ordinal));
        if (targetIndex < 0)
            throw new FileNotFoundException($"在目录 {MigrationsFolder} 中找不到迁移 '{name}' 对应的迁移文件。");

        var targetId = migrationIds[targetIndex];
        // 起点取上一条迁移：dotnet ef migrations script <to> 默认从空库累积生成，
        // 脚本里会混入之前所有迁移的变更；传入 <from> <to> 才能只产出这一条迁移的增量
        var fromId = targetIndex > 0 ? migrationIds[targetIndex - 1] : null;
        var scriptTargets = fromId is null ? $"\"{targetId}\"" : $"\"{fromId}\" \"{targetId}\"";

        var downloadFileName = $"{name}-{DateTime.Now:yyyyMMddHHmmss}-{nameof(Migration)}.sql";
        // 实际落盘的临时文件用 GUID 命名，避免迁移名中的空格/特殊字符污染命令行参数与文件系统
        var scriptTempPath = Path.Combine(MigrationWorkingFolder, $"{Guid.NewGuid():N}.sql");
        try
        {
            await dispatcher.DispatchProcessAsync(DotnetCommand, MigrationWorkingFolder, false,
                $"ef migrations script {scriptTargets} -o \"{scriptTempPath}\" --context {ToMigrateContext}");
            var bytes = await File.ReadAllBytesAsync(scriptTempPath);
            File.Delete(scriptTempPath);
            return (downloadFileName,bytes);
        }
        catch (Exception e)
        {
            if(File.Exists(scriptTempPath))
                File.Delete(scriptTempPath);
            logger.LogError(e.ToString());
            throw;
        }
    }

    public async Task<(string,byte[])> GenerateMigrationScript()
    {
        var downloadFileName = $"{nameof(Migration)}-{DateTime.Now:yyyyMMddHHmmss}.sql";
        var scriptTempPath = Path.Combine(MigrationWorkingFolder, $"{Guid.NewGuid():N}.sql");
        try
        {
            await dispatcher.DispatchProcessAsync(DotnetCommand, MigrationWorkingFolder, false,
                $"ef migrations script --idempotent -o \"{scriptTempPath}\" --context {ToMigrateContext}");
            var bytes = await File.ReadAllBytesAsync(scriptTempPath);
            File.Delete(scriptTempPath);
            return (downloadFileName,bytes);
        }
        catch (Exception e)
        {
            if(File.Exists(scriptTempPath))
                File.Delete(scriptTempPath);
            Console.WriteLine(e);
            throw;
        }
    }

    private static FileInfo GetMigrationFileName(string migrationName)
    {
        var migrationsFolderPath = new DirectoryInfo(MigrationsFolder);
        // 只认 {时间戳}_{Name}.cs 这种主迁移文件，排除同名衍生文件（..._Name.Designer.cs）
        var file = migrationsFolderPath.GetFiles("*.cs", SearchOption.TopDirectoryOnly)
            .FirstOrDefault(f =>
            {
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(f.Name);
                return IsMigrationId(fileNameWithoutExtension) &&
                       fileNameWithoutExtension.EndsWith($"_{migrationName}", StringComparison.Ordinal);
            });
        if(file == null)
            throw new FileNotFoundException($"在目录{MigrationsFolder}下找不到迁移'{migrationName}'相关的文件.");
        return file;
    }
    
    private static void RemoveMigrationFiles(string migrationName)
    {
        var migrationsFolderPath = new DirectoryInfo(MigrationsFolder);
        // 主文件是 {时间戳}_{Name}.cs，元数据文件是 {时间戳}_{Name}.Designer.cs，
        // 去掉 .cs 后分别以 "_Name" 与 "_Name.Designer" 结尾
        var files = migrationsFolderPath.GetFiles("*.cs", SearchOption.TopDirectoryOnly).Where(f =>
        {
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(f.Name);
            return fileNameWithoutExtension.EndsWith($"_{migrationName}", StringComparison.Ordinal) ||
                   fileNameWithoutExtension.EndsWith($"_{migrationName}.Designer", StringComparison.Ordinal);
        });
        foreach (var file in files)
        {
            if(file.Exists)
                file.Delete();
        }
    }

    /// <summary>
    /// 读取 Migrations 目录下真实存在的迁移，并按 EF 迁移顺序排序。
    /// EF 在迁移目录下会同时生成三类文件：
    ///   {时间戳}_{Name}.cs            → 主迁移文件（就是要找的）
    ///   {时间戳}_{Name}.Designer.cs   → 快照元数据（去掉 .cs 后变成 ..._Name.Designer，绝不能当成迁移）
    ///   SysDbContextModelSnapshot.cs  → 当前模型快照（不以时间戳开头）
    /// 注意：Path.GetFileNameWithoutExtension 只去掉最后一个扩展名，
    /// 所以 ".Designer.cs" 会剩下 "xxx.Designer"，必须单独排除。
    /// </summary>
    private static List<string> GetOrderedMigrationIds()
    {
        if (!Directory.Exists(MigrationsFolder))
            throw new DirectoryNotFoundException($"找不到迁移目录 {MigrationsFolder}。");

        return new DirectoryInfo(MigrationsFolder)
            .GetFiles("*.cs", SearchOption.TopDirectoryOnly)
            .Where(f => !f.Name.EndsWith(".Designer.cs", StringComparison.OrdinalIgnoreCase))
            .Select(f => Path.GetFileNameWithoutExtension(f.Name))
            .Where(IsMigrationId)
            .OrderBy(id => id, StringComparer.Ordinal)
            .ToList();
    }

    // 判定是否为 EF 迁移 Id：{纯数字时间戳}_{迁移名称}
    private static bool IsMigrationId(string fileNameWithoutExtension)
    {
        var separatorIndex = fileNameWithoutExtension.IndexOf('_');
        if (separatorIndex <= 0) return false;
        // 名称部分不能为空，也不能再带扩展名残留（如 ..._Name.Designer）
        if (separatorIndex == fileNameWithoutExtension.Length - 1) return false;
        if (fileNameWithoutExtension.Contains('.')) return false;
        return fileNameWithoutExtension[..separatorIndex].All(char.IsAsciiDigit);
    }
}