namespace SocialMedia.Application.ServiceContracts;

public interface IEmailService
{
    Task SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default);
}
