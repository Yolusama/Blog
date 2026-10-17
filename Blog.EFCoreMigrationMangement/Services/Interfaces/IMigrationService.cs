using Blog.EFCoreMigration.Entity;
using Blog.Shared;

namespace Blog.EFCoreMigration.Services.Interfaces;

public interface IMigrationService
{
    public Task<ResponseResult<BasePageQueryResult<Migration>>> Get(string searchKey, int pageIndex, int pageSize);
    public Task<ResponseResult> AddMigration(string name);
    public Task<ResponseResult> Migrate(string id,string name);
    public Task<ResponseResult> RemoveLatest();
    public Task<ResponseResult> RemoveMigration(string id,string name);
}