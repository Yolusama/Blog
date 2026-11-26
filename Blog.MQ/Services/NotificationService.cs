using Blog.MQ.Functional;
using Blog.MQ.Functional.Utils;
using Blog.MQ.Model.DTO;
using Blog.MQ.Model.Entity;
using Blog.MQ.Services.Interfaces;

namespace Blog.MQ.Services;

public class NotificationService(IFreeSql orm,ILogger<NotificationService> logger) : INotificationService
{
    public async Task Make(NotificationDTO notificationDto)
    {
        var notification = notificationDto.ShallowCopy(new Notification());
        notification.CreateTime = DateTime.Now;
        notification.IsRead = false;
        notification.Id = RandomGenerator.SnowflakeId();
        await orm.TransactionAsync(async worker =>
        {
            await worker.Orm.Insert(notification).ExecuteAffrowsAsync();
        }, e => logger.LogError(e.ToString()));
    }
}