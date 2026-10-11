namespace Blog.Application.Attributes;

[AttributeUsage(AttributeTargets.Class |  AttributeTargets.Struct, AllowMultiple = false)]
public class ConfigurationAttribute : Attribute
{
    public string? Key { get; }

    public ConfigurationAttribute()
    {
        Key = null;
    }
    
    public ConfigurationAttribute(string key)
    {
        Key = key;
    }
}