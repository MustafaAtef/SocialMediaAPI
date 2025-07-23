using System;
using SocialMedia.Application.Dtos;

namespace SocialMedia.Application.ServiceContracts;

public interface IEmailProcessorQueue
{
    ValueTask<bool> WriteAsync(EmailDto emailDto, CancellationToken cancellationToken);
    IAsyncEnumerable<EmailDto> ReadAllAsync();
}
