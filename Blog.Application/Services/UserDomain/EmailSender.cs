using System.Net;
using System.Net.Mail;
using Blog.Application.Services.UserDomain.Configuration;
using Blog.Domain.UserDomain.Services.Interfaces;

namespace Blog.Application.Services.UserDomain;

public class EmailSender(EmailConfig config,ILogger<EmailSender> logger) : IEmailSender
{
     public void Send(string emailTo, string subject, string body)
    {
        var smtpServer = config.SmtpServer; // SMTP服务器地址
        var smtpPort = config.SmtpPort; // 通常587是TLS端口，465是SSL端口

        var mailMessage = new MailMessage
        {
            From = new MailAddress(config.Host,config.DisplayName),
            Subject = subject,
            Body = body,
            IsBodyHtml = true // 设置为false发送纯文本
        };

        mailMessage.To.Add(emailTo);

        using var smtpClient = new SmtpClient(smtpServer)
        {
            Port = smtpPort,
            Credentials = new NetworkCredential(config.Host, config.AuthorizationCode),
            EnableSsl = true, // 大多数现代SMTP服务器需要SSL
            DeliveryMethod = SmtpDeliveryMethod.Network
        };
        try
        {
            smtpClient.Send(mailMessage);
            logger.LogTrace($"邮件已成功发送至{emailTo}");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            logger.LogError($"邮件发送失败至{emailTo}，异常信息：{e}");
            throw;
        }
    }

    public async Task SendAsync(string emailTo, string subject, string body)
    {
        var smtpServer = config.SmtpServer; // SMTP服务器地址
        var smtpPort = config.SmtpPort; // 通常587是TLS端口，465是SSL端口

        var mailMessage = new MailMessage
        {
            From = new MailAddress(config.Host,config.DisplayName),
            Subject = subject,
            Body = body,
            IsBodyHtml = true // 设置为false发送纯文本
        };

        mailMessage.To.Add(emailTo);

        using var smtpClient = new SmtpClient(smtpServer)
        {
            Port = smtpPort,
            Credentials = new NetworkCredential(config.Host, config.AuthorizationCode),
            EnableSsl = true, // 大多数现代SMTP服务器需要SSL
            DeliveryMethod = SmtpDeliveryMethod.Network
        };
        try
        {
            await smtpClient.SendMailAsync(mailMessage);
            logger.LogTrace($"邮件已成功发送至{emailTo}");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            logger.LogError($"邮件发送失败至{emailTo}，异常信息：{e}");
            throw;
        }
    }
}