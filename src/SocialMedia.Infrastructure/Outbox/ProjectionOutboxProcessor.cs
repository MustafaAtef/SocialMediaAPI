using System.Data;

using Dapper;

using MediatR;

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using SocialMedia.Application.Abstractions.Data;
using SocialMedia.Core.Abstractions;

namespace SocialMedia.Infrastructure.Outbox;

public class ProjectionOutboxProcessor(
    IServiceProvider serviceProvider,
    ILogger<ProjectionOutboxProcessor> logger) : BackgroundService
{
    private const int BatchSize = 20;
    private const string Savepoint = "msg_sp";

    private static readonly TimeSpan IdleDelay = TimeSpan.FromSeconds(1);

    private static readonly JsonSerializerSettings _jsonSettings = new()
    {
        TypeNameHandling = TypeNameHandling.All
    };

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            int processed = await ProcessBatchAsync(stoppingToken);

            if (processed == BatchSize)
            {
                // Full batch — more messages are very likely queued; loop immediately.
                continue;
            }

            // Partial batch or empty — wait a fixed short interval before the next poll.
            await Task.Delay(IdleDelay, stoppingToken);
        }
    }

    private async Task<int> ProcessBatchAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var publisher = scope.ServiceProvider.GetRequiredService<IPublisher>();
        var connectionFactory = scope.ServiceProvider.GetRequiredService<ISqlConnectionFactory>();
        var transactionContext = scope.ServiceProvider.GetRequiredService<ITransactionContext>();

        // SqlConnectionFactory already opens the connection.
        using var connection = (SqlConnection)connectionFactory.CreateConnection();

        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(
            IsolationLevel.ReadCommitted, cancellationToken);

        // Expose the active connection + transaction so that projection handlers
        // injecting ITransactionContext can enroll in the same atomic unit.
        transactionContext.Connection = connection;
        transactionContext.Transaction = transaction;

        try
        {
            // UPDLOCK + ROWLOCK: hold an update lock on every fetched row for the life of the transaction.
            // READPAST: other workers skip these locked rows instead of blocking.
            var sql = "SELECT TOP (@BatchSize) Id, Type, Payload, OccurredOn " +
                      "FROM OutboxMessage WITH (UPDLOCK, ROWLOCK, READPAST) " +
                      "WHERE ProcessedOn IS NULL " +
                      "ORDER BY OccurredOn";

            List<OutboxMessage> messages = (await connection.QueryAsync<OutboxMessage>(
                new CommandDefinition(sql, new { BatchSize }, transaction: transaction, cancellationToken: cancellationToken)
            )).ToList();

            foreach (var message in messages)
            {
                // Create a savepoint before each message so a failure can be isolated.
                await connection.ExecuteAsync($"SAVE TRANSACTION {Savepoint}", transaction: transaction);

                try
                {
                    var domainEvent = (IDomainEvent?)JsonConvert.DeserializeObject(message.Payload, _jsonSettings);
                    if (domainEvent is not null)
                        await publisher.Publish(domainEvent, cancellationToken);

                    await connection.ExecuteAsync(
                        "UPDATE OutboxMessage SET ProcessedOn = @Now WHERE Id = @Id",
                        new { Now = DateTime.UtcNow, message.Id },
                        transaction: transaction);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to process outbox message {MessageId} of type {Type}.",
                        message.Id, message.Type);

                    // Roll back to the savepoint — undoes any partial work the handler did.
                    await connection.ExecuteAsync(
                        $"ROLLBACK TRANSACTION {Savepoint}", transaction: transaction);

                    // Record the error so this message is not retried indefinitely.
                    await connection.ExecuteAsync(
                        "UPDATE OutboxMessage SET Error = @Error, ProcessedOn = @Now WHERE Id = @Id",
                        new { Error = ex.Message, Now = DateTime.UtcNow, message.Id },
                        transaction: transaction);
                }
            }

            // Commit: persists all projection writes, ProcessedOn / Error updates,
            // and releases all UPDLOCK row locks atomically.
            await transaction.CommitAsync(cancellationToken);
            return messages.Count;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Outbox batch transaction failed — rolling back.");
            await transaction.RollbackAsync(cancellationToken);
            return 0;
        }
    }
}
