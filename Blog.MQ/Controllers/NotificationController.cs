using Blog.MQ.Core;
using Blog.MQ.Core.Attributes;
using Blog.MQ.Model.DTO;
using Blog.MQ.Services.Interfaces;

namespace Blog.MQ.Controllers;

[MQController]
public class NotificationController(INotificationService notificationService) : MQControllerBase
{
    [MQAction]
    public async Task Make(NotificationDTO notificationDto)
    {
        await notificationService.Make(notificationDto);
    }
}