using Blog.Infrastructure.Interfaces;
using Blog.Shared;
using Microsoft.EntityFrameworkCore;

namespace Blog.Infrastructure.Repository;

public class Repository<T,TKey>(SysDbContext context) : IRepository<T, TKey> where  T : AggregateRoot<TKey>
{
    public IQueryable<T> All => context.DbSet<T>().AsQueryable();
    public IQueryable<T> NoTracking => context.DbSet<T>().AsNoTracking();
    public IEnumerable<T> AllList => context.DbSet<T>().AsEnumerable();
    internal DbContext DbContext => context;

    public Task<T?> GetByIdAsync(TKey id)
    {
        return context.DbSet<T>().FindAsync(id).AsTask();
    }

    public Task AddAsync(T entity)
    {
       return context.DbSet<T>().AddAsync(entity).AsTask();
    }

    public void Add(T entity)
    {
        context.DbSet<T>().Add(entity);
    }

    public long InsertIdentity(T entity)
    {
        var idProperty = entity.GetType().GetProperty(nameof(entity.Id));
        var idType = idProperty.PropertyType;
        if (idType != typeof(long) && idType != typeof(int))
            throw new InvalidOperationException("非自增Id了接受类型");
        context.DbSet<T>().Add(entity);
        context.SaveChanges();
        var value = (long)idProperty.GetValue(entity);
        return value;
    }

    public async Task<long> InsertIdentityAsync(T entity)
    {
        var idProperty = entity.GetType().GetProperty(nameof(entity.Id));
        var idType = idProperty.PropertyType;
        if (idType != typeof(long) && idType != typeof(int))
            throw new InvalidOperationException("非自增Id接受类型");
        await context.DbSet<T>().AddAsync(entity);
        await context.SaveChangesAsync();
        var value = (long)idProperty.GetValue(entity);
        return value;
    }

    public void Update(T entity)
    {
        context.DbSet<T>().Update(entity);
    }
    
    public void AddRange(params T[] entities)
    {
       context.DbSet<T>().AddRange(entities);
    }

    public Task AddRangeAsync(params T[] entities)
    {
        return context.DbSet<T>().AddRangeAsync(entities);
    }

    public void UpdateRange(params T[] entities)
    {
        context.DbSet<T>().UpdateRange(entities);
    }

    public void DeleteRange(params T[] entities)
    {
        context.DbSet<T>().RemoveRange(entities);
    }
    
    public void Delete(T entity)
    {
        context.DbSet<T>().Remove(entity);
    }
    
}