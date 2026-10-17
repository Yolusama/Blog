using Blog.Infrastructure.Interfaces;
using Blog.Shared;
using Microsoft.EntityFrameworkCore;

namespace Blog.Infrastructure.Repository;

public class Repository<T,TKey>(SysDbContext context) : IRepository<T, TKey> where  T : AggregateRoot<TKey>
{
    public IQueryable<T> All => DbSet.AsQueryable();
    public IQueryable<T> NoTracking => DbSet.AsNoTracking();
    public IEnumerable<T> AllList => DbSet.AsEnumerable();
    public DbContext DbContext => context;
    public DbSet<T> DbSet { get; } = context.Set<T>();

    public Task<T?> GetByIdAsync(TKey id)
    {
        return DbSet.FindAsync(id).AsTask();
    }

    public Task AddAsync(T entity)
    {
       return DbSet.AddAsync(entity).AsTask();
    }

    public void Add(T entity)
    {
        DbSet.Add(entity);
    }

    public long InsertIdentity(T entity)
    {
        var idProperty = entity.GetType().GetProperty(nameof(entity.Id));
        var idType = idProperty.PropertyType;
        if (idType != typeof(long) && idType != typeof(int))
            throw new InvalidOperationException("非自增Id了接受类型");
        DbSet.Add(entity);
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
        await DbSet.AddAsync(entity);
        await context.SaveChangesAsync();
        var value = (long)idProperty.GetValue(entity);
        return value;
    }

    public void Update(T entity)
    {
        DbSet.Update(entity);
    }
    
    public void AddRange(params T[] entities)
    {
       DbSet.AddRange(entities);
    }

    public Task AddRangeAsync(params T[] entities)
    {
        return DbSet.AddRangeAsync(entities);
    }

    public void UpdateRange(params T[] entities)
    {
        DbSet.UpdateRange(entities);
    }

    public void DeleteRange(params T[] entities)
    {
        DbSet.RemoveRange(entities);
    }
    
    public void Delete(T entity)
    {
        DbSet.Remove(entity);
    }
    
}