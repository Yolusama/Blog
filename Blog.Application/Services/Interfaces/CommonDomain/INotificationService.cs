using Blog.Application.Attributes;
using Blog.Domain.CommonDomain.DTO;

namespace Blog.Application.Services.Interfaces.CommonDomain;

[ServiceInjection]
public interface INotificationService
{
    public Task MakeNotification(NotificationDTO notificationDto);
}