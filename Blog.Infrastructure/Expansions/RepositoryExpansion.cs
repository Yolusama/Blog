using System.Linq.Expressions;
using Blog.Infrastructure.Interfaces;
using Blog.Infrastructure.Repository;
using Blog.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Blog.Infrastructure.Expansions;

public static class RepositoryExpansion
{
    public static void AddRepository(this IServiceCollection services)
    {
        services.AddScoped(typeof(IRepository<,>), typeof(Repository<,>));
    }

    public static IUnitOfWorker CreateUnitOfWorker<T,Tkey>(this IRepository<T,Tkey> repository) where T : AggregateRoot<Tkey>
    {
        return new UnitOfWorker(((Repository<T,Tkey>) repository).DbContext);
    }
    
    public static IQueryable<T> WhereIf<T>(this IQueryable<T> query, bool condition, 
       Expression<Func<T,bool>> predicate) where T : class
    {
        if (condition)
            return query.Where(predicate);
        return query;
    }

    public static IQueryable<T> Page<T>(this IQueryable<T> query, int page, int pageSize) where T : class
    {
        return query.Skip((page - 1) * pageSize).Take(pageSize);
    }
    
    public static IQueryable<T>  Count<T>(this IQueryable<T> query, out long totalCount) where T : class
    {
        totalCount = query.Count();
        return query;
    }
}