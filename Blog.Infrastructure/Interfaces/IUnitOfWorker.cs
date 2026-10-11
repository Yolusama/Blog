using Microsoft.EntityFrameworkCore;

namespace Blog.Infrastructure.Interfaces;

public interface IUnitOfWorker : IDisposable
{
    public DbContext DbContext { get; }
    public Task CommitAsync();
    public Task RollbackAsync();
    public void Commit();
    public void Rollback();
}