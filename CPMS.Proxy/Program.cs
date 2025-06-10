using CPMS.BuildingBlocks.Infrastructure.Logger;
using CPMS.Proxy.Configuration;
using CPMS.Proxy.Services;
using Microsoft.AspNetCore.WebSockets;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services.AddApplicationInsightsTelemetry();

builder.Logging.ClearProviders();

builder.Logging.AddConsole(options =>
{
    options.IncludeScopes = false;
    options.TimestampFormat = "[yyyy-MM-dd HH:mm:ss] ";
});

builder.Logging.AddApplicationInsights(
    configureTelemetryConfiguration: config =>
        config.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"],
    configureApplicationInsightsLoggerOptions: (options) => { }
);

builder.Logging.SetMinimumLevel(LogLevel.Trace);

builder.Logging.AddFilter("Microsoft", LogLevel.Warning);
builder.Logging.AddFilter("System", LogLevel.Warning);
builder.Logging.AddFilter("CPMS", LogLevel.Information);
builder.Logging.AddFilter("CPMS.BuildingBlocks.Infrastructure.Logger", LogLevel.Information);
builder.Logging.AddFilter("CPMS.Proxy", LogLevel.Information);

#region Services

builder.Services.AddMemoryCache();
builder.Services.AddSingleton<ILoggerService, LoggerService>();
builder.Services.AddSingleton<IAuthorizationCache, AuthorizationCache>();
builder.Services.AddHttpClient<ICpmsClient, CpmsClient>((serviceProvider, client) =>
{
    var baseUrl = configuration["CpmsApi:BaseUrl"];
    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(120);
});

#endregion

// change once in prod
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services.AddWebSockets(options =>
{
    options.KeepAliveInterval = TimeSpan.FromMinutes(2);
});

builder.Services.AddControllers();

builder.Services.AddHealthChecks()
    .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("Proxy is running"));

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILoggerService>();
logger.Info("Proxy application starting up");

app.UseWebSockets();

app.UseCors("AllowAll");

app.UseOCPPMiddleware();

app.UseRouting();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

await app.RunAsync();