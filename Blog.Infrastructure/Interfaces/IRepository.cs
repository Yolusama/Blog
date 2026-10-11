using Blog.Shared;

namespace Blog.Infrastructure.Interfaces;

public interface IRepository<T, TKey> where T : AggregateRoot<TKey>
{
    /// <summary>
    /// 实体的IQueryable表达
    /// </summary>
    public IQueryable<T> All { get; }
    /// <summary>
    /// 取消EFCoreTracking的IQueryAble表达
    /// </summary>
    public IQueryable<T> NoTracking { get; }
    /// <summary>
    /// 直接加载到内存
    /// </summary>
    public IEnumerable<T> AllList { get; }
    public Task<T?> GetByIdAsync(TKey id);
    public Task AddAsync(T entity);
    public void Add(T entity);
    public long InsertIdentity(T entity);
    public Task<long> InsertIdentityAsync(T entity);
    public void Delete(T entity);
    public void Update(T entity);
    public void AddRange(params T[] entities);
    public Task AddRangeAsync(params T[] entities);
    public void UpdateRange(params T[] entities);
    public void DeleteRange(params T[] entities);
}