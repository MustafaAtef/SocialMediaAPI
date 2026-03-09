namespace SocialMedia.Infrastructure.Outbox;

public class EmailOutboxMessage
{
    public Guid Id { get; set; }
    public string To { get; set; } = null!;
    public string Subject { get; set; } = null!;
    public string HtmlBody { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedOn { get; set; }
    public string? Error { get; set; }
}
