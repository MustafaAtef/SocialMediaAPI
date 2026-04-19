using FluentValidation;

using SocialMedia.Application.Abstractions.Behaviors;
using SocialMedia.Application.Options;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace SocialMedia.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {

        services.Configure<ExpireDurationsOptions>(configuration.GetSection("durations"));
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly)
                .AddOpenBehavior(typeof(ValidationBehavior<,>))
                .AddOpenBehavior(typeof(CurrentUserQueryBehavior<,>));
        });
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        return services;
    }
}
