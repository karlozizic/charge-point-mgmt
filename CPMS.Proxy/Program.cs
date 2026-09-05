using CPMS.BuildingBlocks.Infrastructure.Logger;
using CPMS.Proxy.Middleware;
using CPMS.Proxy.Services;
using Microsoft.AspNetCore.WebSockets;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole(options =>
{
    options.IncludeScopes = false;
    options.TimestampFormat = "[yyyy-MM-dd HH:mm:ss] ";
});
builder.Logging.SetMinimumLevel(LogLevel.Trace);
builder.Logging.AddFilter("Microsoft", LogLevel.Warning);
builder.Logging.AddFilter("System", LogLevel.Warning);
builder.Logging.AddFilter("CPMS", LogLevel.Information);

builder.Services.AddMemoryCache();
builder.Services.AddSingleton<ILoggerService, LoggerService>();
builder.Services.AddSingleton<IAuthorizationCache, AuthorizationCache>();
builder.Services.AddHttpClient<ICpmsClient, CpmsClient>(client =>
{
    var baseUrl = builder.Configuration["CpmsApi:BaseUrl"]
        ?? throw new InvalidOperationException("CpmsApi:BaseUrl is not configured.");
    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(120);
});

builder.Services.AddWebSockets(options => options.KeepAliveInterval = TimeSpan.FromMinutes(2));

// Known gap: this only proves the process is up, not that it can reach the API.
builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseWebSockets();
app.UseOcppMiddleware();
app.MapHealthChecks("/health");

await app.RunAsync();
