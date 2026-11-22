namespace Blog.Application.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class NoAuditLogAttribute : Attribute
{
    
}