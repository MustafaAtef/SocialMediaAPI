using SocialMedia.Application.Dtos;

namespace SocialMedia.Application.ServiceContracts;

public interface IEmailService
{
    Task SendEmailAsync(EmailDto emailDto);
}
