namespace Blog.Application.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
public class ServiceInjectionAttribute : Attribute
{
    public ServiceLifetime ServiceLifetime { get; set; }
    public ServiceInjectionAttribute(ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
    {
        ServiceLifetime = serviceLifetime;
    }
}