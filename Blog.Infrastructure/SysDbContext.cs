using Blog.Shared.Functional;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Blog.Infrastructure;

public partial class SysDbContext : DbContext
{
    private readonly IMemoryCache _cache;
    private readonly List<string> _cacheKeys;
    private readonly string _specificKeyPrefix = RandomGenerator.Guid();

    public DbSet<TE> DbSet<TE>() where TE : class
    {
        var type = typeof(TE);
        var key = $"{_specificKeyPrefix}_{GetType().Name}_{type.Name}";
        if (!_cache.TryGetValue(key, out DbSet<TE> dbSet))
        {
            dbSet = Set<TE>();
            _cache.Set(key, dbSet);
            _cacheKeys.Add(key);
        }

        return dbSet;
    }

    public SysDbContext(DbContextOptions<SysDbContext> options, IMemoryCache cache) : base(options)
    {
        _cache = cache;
        _cacheKeys = new List<string>();
    }

    public override void Dispose()
    {
        base.Dispose();
        foreach (var key in _cacheKeys)
            _cache.Remove(key);
    }
}