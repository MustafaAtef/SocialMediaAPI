using System.Text;
using EducationCenter.Core.RepositoryContracts;
using EducationCenterAPI.Repositories;
using SocialMedia.Application.ServiceContracts;
using SocialMedia.Application.Services;
using SocialMedia.Infrastructure.Auth;
using SocialMedia.Infrastructure.Database;
using SocialMedia.Infrastructure.Email;
using SocialMedia.Infrastructure.FileUploading;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using SocialMedia.Application.Service;
using SocialMedia.WebApi.Middlewares;
using SocialMedia.WebApi.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddDatabase(builder.Configuration);
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("jwt"));
builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection("email"));
builder.Services.AddHttpContextAccessor();
builder.Services.AddSignalR();
builder.Services.AddSingleton<IEmailProcessorQueue, EmailProcessorQueue>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IFileUploader, ServerFileUploader>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<IReactService, ReactService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<Supabase.Client>(_ => new Supabase.Client(
    builder.Configuration["Supabase:Url"],
    builder.Configuration["Supabase:Key"],
     new()
     {
         AutoRefreshToken = true,
         AutoConnectRealtime = true,
     })
);

var jwtOptions = builder.Configuration.GetSection("jwt").Get<JwtOptions>();
builder.Services.AddAuthentication().AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    if (jwtOptions is null) throw new Exception();
    // save the authentication token to authentication properties so it can be accessed from httpContext object
    options.SaveToken = true;
    options.MapInboundClaims = false;
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidIssuer = jwtOptions.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtOptions.Audience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
        ValidateLifetime = true,
        // The default value of ClockSkew is 5 minutes. That means if you haven't set it, your token will be still valid for up to 5 minutes. If you want to expire your token on the exact time; you'd need to set ClockSkew to zero
        ClockSkew = TimeSpan.Zero,
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                // If the request is for SignalR hub
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

builder.Services.AddCors(builder =>
{
    builder.AddDefaultPolicy(policyBuilder =>
    {
        policyBuilder.WithOrigins("http://127.0.0.1:5500")
            .AllowAnyHeader()
            .AllowAnyMethod().AllowCredentials();
    });
});

builder.Services.AddHostedService<EmailProcessorService>();

var app = builder.Build();

app.UseGlobalErrorHandling();
app.UseStaticFiles();
app.UseCors();
app.MapControllers();
app.MapHub<ChatHub>("/hubs/chat");
app.Run();
