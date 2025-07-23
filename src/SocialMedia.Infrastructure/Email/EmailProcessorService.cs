using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SocialMedia.Application.ServiceContracts;

namespace SocialMedia.Infrastructure.Email;

public class EmailProcessorService : BackgroundService
{
    private readonly IEmailProcessorQueue _emailProcessorQueue;
    private readonly IServiceProvider _serviceProvider;

    public EmailProcessorService(IEmailProcessorQueue emailProcessorChannel, IServiceProvider serviceProvider)
    {
        _emailProcessorQueue = emailProcessorChannel;
        _serviceProvider = serviceProvider;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var email in _emailProcessorQueue.ReadAllAsync().WithCancellation(stoppingToken))
        {
            using var serviceScope = _serviceProvider.CreateScope();
            var emailService = serviceScope.ServiceProvider.GetRequiredService<IEmailService>();
            await emailService.SendEmailAsync(email);
        }
    }
}
