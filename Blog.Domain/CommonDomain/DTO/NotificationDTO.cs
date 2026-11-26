namespace Blog.Domain.CommonDomain.DTO;

public class NotificationDTO
{
    public string Content { get; set; }
    public int? SenderId { get; set; }
    public int ReceiverId { get; set; }
    public int Type { get; set; }
}