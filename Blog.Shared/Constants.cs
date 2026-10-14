using System.Text.Json;
using Blog.Shared.Functional;

namespace Blog.Shared;

public static class Constants
{
    public const string Redis = nameof(Redis);
    public const string SySDb = nameof(SySDb);
    public const string MigrationDb = nameof(MigrationDb);
    public const string FileResponseHeader = "application/octet-stream";
    public const string DefaultUserAvatar = "default-user.png";
    public const int ChannelCapacity = 1024;
    public const int EOF = -1;
    
    public static TimeSpan LoginExpire { get; } = TimeSpan.FromDays(7);
    public static TimeSpan RegisterExpire { get; } = TimeSpan.FromMinutes(3);
    public static JsonSerializerOptions SmallCamelcase { get; } = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };
}