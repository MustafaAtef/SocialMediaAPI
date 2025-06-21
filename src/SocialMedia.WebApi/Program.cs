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

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddDatabase(builder.Configuration);
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("jwt"));
builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection("email"));
builder.Services.AddHttpContextAccessor();


builder.Services.AddTransient<IAuthService, AuthService>();
builder.Services.AddTransient<IPasswordHasher, PasswordHasher>();
builder.Services.AddTransient<IJwtService, JwtService>();
builder.Services.AddTransient<IEmailService, EmailService>();
builder.Services.AddTransient<IFileUploader, ServerFileUploader>();
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
});

var app = builder.Build();

app.UseStaticFiles();

app.MapControllers();

app.Run();
