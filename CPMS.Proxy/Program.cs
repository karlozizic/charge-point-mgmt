using CPMS.BuildingBlocks.Infrastructure.Logger;
using CPMS.Proxy.Configuration;
using CPMS.Proxy.Services;
using Microsoft.AspNetCore.WebSockets;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

#region Services

builder.Services.AddSingleton<ILoggerService, LoggerService>();
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

app.UseWebSockets();

app.UseCors("AllowAll");

app.UseOCPPMiddleware();

app.UseRouting();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

await app.RunAsync();