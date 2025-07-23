using System;
using System.Threading.Channels;
using SocialMedia.Application.Dtos;
using SocialMedia.Application.ServiceContracts;

namespace SocialMedia.Infrastructure.Email;

public class EmailProcessorQueue : IEmailProcessorQueue
{
    private readonly Channel<EmailDto> _channel;
    public EmailProcessorQueue()
    {
        _channel = Channel.CreateBounded<EmailDto>(new BoundedChannelOptions(100)
        {
            SingleReader = true,
            SingleWriter = false
        });
    }

    public async ValueTask<bool> WriteAsync(EmailDto emailDto, CancellationToken cancellationToken)
    {
        while (await _channel.Writer.WaitToWriteAsync(cancellationToken))
        {
            return _channel.Writer.TryWrite(emailDto);
        }
        return false;
    }

    public IAsyncEnumerable<EmailDto> ReadAllAsync() => _channel.Reader.ReadAllAsync();
}
