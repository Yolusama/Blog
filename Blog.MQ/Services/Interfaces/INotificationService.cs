using Blog.MQ.Model.DTO;

namespace Blog.MQ.Services.Interfaces;

public interface INotificationService
{
    public Task Make(NotificationDTO notificationDto);
}