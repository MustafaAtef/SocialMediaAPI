namespace SocialMedia.Application.ServiceContracts;

public interface IEmailOutboxWriter
{
    void QueueVerificationEmail(string to, string token, DateTime expiry);
    void QueuePasswordResetEmail(string to, string token, DateTime expiry);
}
