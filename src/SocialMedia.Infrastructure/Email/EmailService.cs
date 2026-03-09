using System.Security.Authentication;

using SocialMedia.Application.ServiceContracts;

using MailKit.Net.Smtp;
using MailKit.Security;

using Microsoft.Extensions.Options;

using MimeKit;
using MimeKit.Text;

namespace SocialMedia.Infrastructure.Email;

public class EmailService : IEmailService
{
    private readonly EmailOptions _emailOptions;

    public EmailService(IOptions<EmailOptions> emailOptions)
    {
        _emailOptions = emailOptions.Value;
    }

    public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        var email = new MimeMessage();
        email.From.Add(MailboxAddress.Parse(_emailOptions.From));
        email.To.Add(MailboxAddress.Parse(to));
        email.Subject = subject;
        email.Body = new TextPart(TextFormat.Html) { Text = htmlBody };
        await _sendEmailAsync(email);
    }

    private async Task _sendEmailAsync(MimeMessage email)
    {
        using var smtpClient = new SmtpClient();
        smtpClient.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;
        smtpClient.CheckCertificateRevocation = false;
        await smtpClient.ConnectAsync(_emailOptions.Smtp, _emailOptions.Port, SecureSocketOptions.Auto);
        await smtpClient.AuthenticateAsync(_emailOptions.Username, _emailOptions.Password);
        await smtpClient.SendAsync(email);
        await smtpClient.DisconnectAsync(true);
    }
}
