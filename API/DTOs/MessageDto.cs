namespace API.DTOs;

public class MessageDto
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public int SenderUserId { get; set; }
    public int RecipientUserId { get; set; }
    public string SenderUsername { get; set; } = string.Empty;
    public string RecipientUsername { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
