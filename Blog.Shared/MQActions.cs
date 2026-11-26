namespace Blog.Shared;

public static class MQActions
{
    public const string ArticleQueue = "Article";
    public const string ArticleAuditAction = "Article.Audit";
    public const string NotificationQueue = "Notification";
    public const string NotificationMakeAction = "Notification.Make";
}