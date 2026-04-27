using System;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

using Respawn;
using Respawn.Graph;

using SocialMedia.Application.Abstractions.Data;
using SocialMedia.Infrastructure.Data;
using SocialMedia.Infrastructure.Database;

using Testcontainers.MsSql;

using Xunit;

namespace SocialMedia.IntegrationTests.Infrastructure;


public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MsSqlContainer _msSqlContainer = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest")
        .WithCleanUp(true)
        .Build();
    public HttpClient HttpClient { get; private set; } = null!;
    private SqlConnection _dbConnection = null!;
    private Respawner _respawner = null!;
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        var testConnectionString = _msSqlContainer.GetConnectionString();

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<AppDbContext>));
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(testConnectionString);
            });

            services.RemoveAll(typeof(IHostedService));

            services.RemoveAll(typeof(ISqlConnectionFactory));
            services.AddSingleton<ISqlConnectionFactory>(new SqlConnectionFactory(testConnectionString));
        });
    }

    public async ValueTask InitializeAsync()
    {
        await _msSqlContainer.StartAsync();
        var testConnectionString = _msSqlContainer.GetConnectionString();

        HttpClient = CreateClient();
        _dbConnection = new SqlConnection(testConnectionString);
        await _dbConnection.OpenAsync();
        _respawner = await Respawner.CreateAsync(_dbConnection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.SqlServer,
            SchemasToInclude = new[] { "dbo" },
            TablesToIgnore = new Table[]
            {
                "__EFMigrationsHistory"
            }
        });

    }

    public async Task ResetDatabaseAsync()
    {
        await _respawner.ResetAsync(_dbConnection);
    }

    public override async ValueTask DisposeAsync()
    {
        await _dbConnection.DisposeAsync();
        await base.DisposeAsync();
        await _msSqlContainer.DisposeAsync();
    }
}
