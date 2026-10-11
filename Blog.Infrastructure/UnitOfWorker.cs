using Blog.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Blog.Infrastructure;

internal class UnitOfWorker : IUnitOfWorker
{
    private readonly IDbContextTransaction _dbTransaction;
    internal DbContext _dbContext;

    internal UnitOfWorker(DbContext dbContext)
    {
        _dbContext = dbContext;
        _dbTransaction = _dbContext.Database.BeginTransaction();
    }
    public void Dispose()
    {
        _dbTransaction.Dispose();
    }

    public DbContext DbContext => _dbContext;
    
    public Task CommitAsync()
    {
       return _dbTransaction.CommitAsync();
    }

    public Task RollbackAsync()
    {
        return  _dbTransaction.RollbackAsync();
    }

    public void Commit()
    {
        _dbTransaction.Commit();
    }

    public void Rollback()
    {
        _dbTransaction.Rollback();
    }
}