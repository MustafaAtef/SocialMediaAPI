
using System.Threading.RateLimiting;

using Microsoft.OpenApi;

using SocialMedia.WebApi.Filters;
using SocialMedia.WebApi.Middlewares;
using SocialMedia.WebApi.options;

namespace SocialMedia.WebApi;

public static class DependencyInjection
{
    public static IServiceCollection AddWebApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.SuppressModelStateInvalidFilter = true;
    });
        services.AddSignalR();
        services.AddCors(corsOptions =>
         {
             corsOptions.AddDefaultPolicy(policy =>
             {
                 policy
                     .AllowAnyOrigin()
                     .AllowAnyHeader()
                     .AllowAnyMethod();
             });
         });

        services.AddHttpContextAccessor();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();
        services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token."
    });

    // Add operation filter to apply security only to endpoints with [Authorize] attribute
    c.OperationFilter<AuthorizeCheckOperationFilter>();
    c.AddSignalRSwaggerGen();
});
        var supabaseSettings = configuration.GetSection("Supabase").Get<SupabaseOptions>();
        if (supabaseSettings != null)
        {
            services.AddSingleton(provider => new Supabase.Client(supabaseSettings.Url, supabaseSettings.Key, new Supabase.SupabaseOptions
            {
                AutoRefreshToken = true,
                AutoConnectRealtime = true,
            }));
        }

        return services;
    }

    private static IServiceCollection AddRateLimiting(this IServiceCollection services)
    {
        // services.AddRateLimiter(options =>
        // {
        //     options.AddPolicy("SlidingWindowPolicy", context =>
        //         RateLimitPartition.GetSlidingWindowLimiter(partitionKey: context.User., factory: partition => new SlidingWindowRateLimiterOptions
        //         {
        //             PermitLimit = 100,
        //             Window = TimeSpan.FromMinutes(1),
        //             SegmentsPerWindow = 6,
        //             QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
        //             QueueLimit = 0
        //         }));
        // });

        return services;
    }
}
