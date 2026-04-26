using Microsoft.Extensions.DependencyInjection;

using Dapper;

using MediatR;

using Microsoft.Data.SqlClient;

using Newtonsoft.Json;

using SocialMedia.Application.Abstractions.Data;
using SocialMedia.Core.Abstractions;
using SocialMedia.Infrastructure.Database;
using SocialMedia.Infrastructure.Outbox;

using Xunit;

namespace SocialMedia.IntegrationTests.Infrastructure;

[Collection("IntegrationTests")]
public abstract class BaseIntegrationTest : IAsyncLifetime
{
    private static readonly JsonSerializerSettings JsonSerializerSettings = new()
    {
        TypeNameHandling = TypeNameHandling.All
    };

    private readonly IntegrationTestWebAppFactory _factory;

    protected HttpClient HttpClient { get; }
    protected AppDbContext DbContext { get; }

    protected BaseIntegrationTest(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
        HttpClient = factory.HttpClient;

        var scope = factory.Services.CreateScope();
        DbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    }

    public async ValueTask InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
    }

    protected async Task FlushProjectionOutboxAsync()
    {
        using var scope = _factory.Services.CreateScope();

        var publisher = scope.ServiceProvider.GetRequiredService<IPublisher>();
        var connectionFactory = scope.ServiceProvider.GetRequiredService<ISqlConnectionFactory>();
        var transactionContext = scope.ServiceProvider.GetRequiredService<ITransactionContext>();

        using var connection = (SqlConnection)connectionFactory.CreateConnection();
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync();

        transactionContext.Connection = connection;
        transactionContext.Transaction = transaction;

        var messages = (await connection.QueryAsync<OutboxMessage>(
            "SELECT Id, Type, Payload, OccurredOn FROM OutboxMessage WHERE ProcessedOn IS NULL ORDER BY OccurredOn",
            transaction: transaction)).ToList();

        foreach (var message in messages)
        {
            try
            {
                var domainEvent = JsonConvert.DeserializeObject<IDomainEvent>(
                    message.Payload,
                    JsonSerializerSettings);

                if (domainEvent is not null)
                {
                    await publisher.Publish(domainEvent);
                }

                await connection.ExecuteAsync(
                    "UPDATE OutboxMessage SET ProcessedOn = @ProcessedOn WHERE Id = @Id",
                    new { ProcessedOn = DateTime.UtcNow, message.Id },
                    transaction);
            }
            catch (Exception ex)
            {
                await connection.ExecuteAsync(
                    "UPDATE OutboxMessage SET Error = @Error, ProcessedOn = @ProcessedOn WHERE Id = @Id",
                    new
                    {
                        Error = ex.Message,
                        ProcessedOn = DateTime.UtcNow,
                        message.Id
                    },
                    transaction);
            }
        }

        await transaction.CommitAsync();
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}