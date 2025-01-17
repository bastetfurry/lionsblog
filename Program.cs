using LionsBlog;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();

var app = builder.Build();

using var loggerFactory = LoggerFactory.Create(loggingBuilder => loggingBuilder
    .SetMinimumLevel(LogLevel.Trace)
    .AddConsole());

var logger = loggerFactory.CreateLogger("Startup");

LionsBlog.ConfigurationProvider.SetConfiguration(builder.Configuration);

var configuration = LionsBlog.ConfigurationProvider.GetConfiguration();

if(!System.IO.Directory.Exists(configuration.ImageDirectory))
{
    System.IO.Directory.CreateDirectory(configuration.ImageDirectory);
}

var database = new Database();

if (configuration.RecoverAdminUser)
{
    logger.LogCritical("Admin recovery active! Remember to set RecoverAdminUser to false when done!");
    database.createAdminUser();
}

#if DEBUG
    logger.LogWarning("Debug build, exception to browser active!");
    app.UseDeveloperExceptionPage();
#endif

if(configuration.EnforceHTTPS)
{
    logger.LogInformation("HTTPS is enforced.");
    app.UseHttpsRedirection();
}

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
