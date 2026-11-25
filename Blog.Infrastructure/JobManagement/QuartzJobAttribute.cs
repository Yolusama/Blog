namespace Blog.Infrastructure.JobManagement;

[AttributeUsage(AttributeTargets.Class)]
public class QuartzJobAttribute : Attribute
{
    public required string CronExpression { get; set; }
    public string JobKey { get; set; }
    public string JobTriggerId {get; set;}
}