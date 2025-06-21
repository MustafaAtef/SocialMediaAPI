namespace SocialMedia.Application.ServiceContracts;

public interface IEmailService
{
    Task SendEmailVerificationAsync(string toEmail, string token, DateTime tokenExpiration);
    Task SendPasswordResetAsync(string toEmail, string token, DateTime tokenExpiration);
}
