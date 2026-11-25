using System.Reflection;
using Blog.Infrastructure.JobManagement;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace Blog.Infrastructure.Expansions;

public static class JobExpansion
{
    public static void AddJobs(this IServiceCollection services)
    {
        var jobTypes = Assembly.GetEntryAssembly()
            .GetTypes()
            .Where(t => t.IsAssignableTo(typeof(IJob)) && t.IsDefined(typeof(QuartzJobAttribute), false));
        services.AddQuartz(q =>
        {
            foreach (var jobType in jobTypes)
            {
                var jobAttr = jobType.GetCustomAttribute<QuartzJobAttribute>()!;
                var jobKey = !string.IsNullOrEmpty(jobAttr.JobKey?.Trim()) ? jobAttr.JobKey.Trim() : jobType.Name;
                var jobTriggerId = !string.IsNullOrEmpty(jobAttr.JobTriggerId?.Trim()) ? jobAttr.JobTriggerId.Trim() :
                    $"Trigger_{jobType.Name}";
                q.CreateJob(jobKey,jobTriggerId,jobAttr.CronExpression,jobType);
                services.AddScoped(jobType);
            }
        });

        services.AddQuartzHostedService(q =>
        {
            q.StartDelay = TimeSpan.FromSeconds(3);
            q.WaitForJobsToComplete = true;
        });
    }

    private static void CreateJob(this IServiceCollectionQuartzConfigurator quartz,string jobKey,
        string jobTrigger,string cronExpression,Type jobType)
    {
        if(!jobType.IsAssignableTo(typeof(IJob)))
            throw new InvalidOperationException($"{jobType} is not implemented to {typeof(IJob)}");
        var key = new JobKey(jobKey);
        quartz.AddJob(jobType,key);

        quartz.AddTrigger(opts => opts.ForJob(jobKey)
            .WithIdentity(jobTrigger)
            .WithCronSchedule(cronExpression)
        );
    }
}