using Microsoft.Extensions.Options;

using SocialMedia.Application.ServiceContracts;
using SocialMedia.Infrastructure.Database;
using SocialMedia.Infrastructure.Outbox;

namespace SocialMedia.Infrastructure.Email;

public sealed class EmailOutboxWriter(
    AppDbContext dbContext,
    IOptions<EmailOptions> emailOptions) : IEmailOutboxWriter
{
    private readonly EmailOptions _options = emailOptions.Value;

    public void QueueVerificationEmail(string to, string token, DateTime expiry)
    {
        dbContext.EmailOutboxMessages.Add(new EmailOutboxMessage
        {
            Id = Guid.NewGuid(),
            To = to,
            Subject = "Activate Your Account",
            HtmlBody = EmailTemplate.ActivateTemplate(_options.BuildVerificationUrl(token), expiry),
            CreatedAt = DateTime.UtcNow
        });
    }

    public void QueuePasswordResetEmail(string to, string token, DateTime expiry)
    {
        dbContext.EmailOutboxMessages.Add(new EmailOutboxMessage
        {
            Id = Guid.NewGuid(),
            To = to,
            Subject = "Reset Your Password",
            HtmlBody = EmailTemplate.ResetPasswordTemplate(_options.BuildPasswordResetUrl(token), expiry),
            CreatedAt = DateTime.UtcNow
        });
    }
}
