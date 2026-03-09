using System.Data;

using Dapper;

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using SocialMedia.Application.Abstractions.Data;
using SocialMedia.Application.ServiceContracts;

namespace SocialMedia.Infrastructure.Outbox;

public sealed class EmailOutboxProcessor(
    IServiceProvider serviceProvider,
    ILogger<EmailOutboxProcessor> logger) : BackgroundService
{
    private const int BatchSize = 10;
    private static readonly TimeSpan IdleDelay = TimeSpan.FromSeconds(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            int processed = await ProcessBatchAsync(stoppingToken);

            if (processed < BatchSize)
                await Task.Delay(IdleDelay, stoppingToken);
        }
    }

    private async Task<int> ProcessBatchAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
        var connectionFactory = scope.ServiceProvider.GetRequiredService<ISqlConnectionFactory>();

        using var connection = (SqlConnection)connectionFactory.CreateConnection();
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(
            IsolationLevel.ReadCommitted, cancellationToken);

        try
        {
            var sql = """
                SELECT TOP (@BatchSize) Id, [To], Subject, HtmlBody, CreatedAt
                FROM EmailOutboxMessages WITH (UPDLOCK, ROWLOCK, READPAST)
                WHERE ProcessedOn IS NULL
                ORDER BY CreatedAt
                """;

            var messages = (await connection.QueryAsync<EmailOutboxMessage>(
                new CommandDefinition(sql, new { BatchSize }, transaction: transaction, cancellationToken: cancellationToken)
            )).ToList();

            foreach (var message in messages)
            {
                try
                {
                    await emailService.SendAsync(message.To, message.Subject, message.HtmlBody, cancellationToken);

                    await connection.ExecuteAsync(
                        "UPDATE EmailOutboxMessages SET ProcessedOn = @Now WHERE Id = @Id",
                        new { Now = DateTime.UtcNow, message.Id },
                        transaction: transaction);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to send email outbox message {MessageId} to {To}.",
                        message.Id, message.To);

                    await connection.ExecuteAsync(
                        "UPDATE EmailOutboxMessages SET Error = @Error, ProcessedOn = @Now WHERE Id = @Id",
                        new { Error = ex.Message, Now = DateTime.UtcNow, message.Id },
                        transaction: transaction);
                }
            }

            await transaction.CommitAsync(cancellationToken);
            return messages.Count;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Email outbox batch transaction failed — rolling back.");
            await transaction.RollbackAsync(cancellationToken);
            return 0;
        }
    }
}
