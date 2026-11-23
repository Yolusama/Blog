namespace Blog.Infrastructure.JobManagement;

[AttributeUsage(AttributeTargets.Class)]
public class QuartzJobAttribute : Attribute
{
    public required string CronExpression { get; set; }
    public required string JobKey { get; set; }
    public required string JobTriggerId {get; set;}
}