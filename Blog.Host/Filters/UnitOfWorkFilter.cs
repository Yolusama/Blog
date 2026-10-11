using Blog.Application.Attributes;
using Blog.Infrastructure;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Blog.Services.Filters;

public class UnitOfWorkFilter(ILogger<UnitOfWorkFilter> logger) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var dbContext = context.HttpContext.RequestServices.GetRequiredService<SysDbContext>();
        var unitOfWorkAttr = context.ActionDescriptor.EndpointMetadata.OfType<UnitOfWorkAttribute>().FirstOrDefault();

        if (unitOfWorkAttr == null)
        {
            var executedContext = await next.Invoke();
            if (executedContext.Exception == null && dbContext.ChangeTracker.HasChanges())
                await dbContext.SaveChangesAsync();
        }
        else
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync();
            try
            {
                var executedContext = await next.Invoke();
                if (executedContext.Exception != null)
                    throw executedContext.Exception;

                if (dbContext.ChangeTracker.HasChanges())
                    await dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception e)
            {
                logger.LogError(e, "UnitOfWork transaction failed");
                await transaction.RollbackAsync();
            }
        }
    }
}