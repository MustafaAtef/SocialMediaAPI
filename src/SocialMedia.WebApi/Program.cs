using Microsoft.EntityFrameworkCore;

using Serilog;

using SocialMedia.Application;
using SocialMedia.Infrastructure;
using SocialMedia.Infrastructure.Database;
using SocialMedia.WebApi;
using SocialMedia.WebApi.Hubs;
using SocialMedia.WebApi.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) =>
    configuration
        .ReadFrom.Configuration(context.Configuration)
);

builder.Services.AddApplication(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddWebApi(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        if (dbContext.Database.GetPendingMigrations().Any())
        {
            dbContext.Database.Migrate();
        }
    }
    catch (Exception) { }
}

app.UseRequestContextLogging();
app.UseSerilogRequestLogging();
app.UseExceptionHandler();
app.UseStaticFiles();
app.UseCors();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "SocialMediaAPI v1"));
}
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<ChatHub>("/hubs/chat");
app.Run();
