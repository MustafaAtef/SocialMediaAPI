using System.Text;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

using SocialMedia.Application.Abstractions.Data;
using SocialMedia.Application.ServiceContracts;
using SocialMedia.Application.Services;
using SocialMedia.Core.RepositoryContracts;
using SocialMedia.Infrastructure.Auth;
using SocialMedia.Infrastructure.Data;
using SocialMedia.Infrastructure.Database;
using SocialMedia.Infrastructure.Email;
using SocialMedia.Infrastructure.FileUploading;
using SocialMedia.Infrastructure.Outbox;
using SocialMedia.Infrastructure.Repositories;

namespace SocialMedia.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection("jwt"));
        services.Configure<EmailOptions>(configuration.GetSection("email"));

        services
            .AddDatabase(configuration)
            .AddRepositories()
            .AddApplicationServices()
            .AddDataAccess()
            .AddAuthServices(configuration)
            .AddEmailServices()
            .AddFileUploadServices()
            .AddBackgroundServices();

        return services;
    }

    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }

    private static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();

        return services;
    }

    private static IServiceCollection AddDataAccess(this IServiceCollection services)
    {
        services.AddScoped<ISqlConnectionFactory, SqlConnectionFactory>();
        services.AddScoped<ITransactionContext, TransactionContext>();

        return services;
    }

    private static IServiceCollection AddAuthServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtService, JwtService>();

        var jwtOptions = configuration.GetSection("jwt").Get<JwtOptions>()
            ?? throw new InvalidOperationException("JWT options are not configured.");

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.SaveToken = true;
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtOptions.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) &&
                            path.StartsWithSegments("/hubs"))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });

        return services;
    }

    private static IServiceCollection AddEmailServices(this IServiceCollection services)
    {
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IEmailOutboxWriter, EmailOutboxWriter>();

        return services;
    }

    private static IServiceCollection AddFileUploadServices(this IServiceCollection services)
    {
        services.AddScoped<IFileUploader, ServerFileUploader>();
        services.AddScoped<ServerFileUploader>();

        return services;
    }

    private static IServiceCollection AddBackgroundServices(this IServiceCollection services)
    {
        services.AddHostedService<EmailOutboxProcessor>();
        services.AddHostedService<ProjectionOutboxProcessor>();

        return services;
    }

}
