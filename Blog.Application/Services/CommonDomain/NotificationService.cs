using Blog.Application.Services.Interfaces.CommonDomain;
using Blog.Application.Services.UserDomain.Configuration;
using Blog.Domain.CommonDomain.DTO;
using Blog.Infrastructure.Interfaces;
using Blog.Shared;

namespace Blog.Application.Services.CommonDomain;

public class NotificationService(IMQProducer mqProducer,RabbitMQConfig config) : INotificationService
{
    public async Task MakeNotification(NotificationDTO notificationDto)
    {
        await mqProducer.Produce(MQActions.NotificationQueue, config.ClientProvideName,
            MQActions.NotificationMakeAction, notificationDto);
    }
}