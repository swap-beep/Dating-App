using System.ComponentModel.DataAnnotations.Schema;

namespace API.Entities;

public class Message
{
    public int Id { get; set; }
    public required string Content { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(Sender))]
    public int SenderUserId { get; set; }
    public AppUser Sender { get; set; } = null!;

    [ForeignKey(nameof(Recipient))]
    public int RecipientUserId { get; set; }
    public AppUser Recipient { get; set; } = null!;
}
