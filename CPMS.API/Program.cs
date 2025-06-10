using CPMS.API.Projections;
using CPMS.API.Repositories;
using CPMS.API.Services;
using CPMS.BuildingBlocks.Infrastructure.Logger;
using Marten;
using Marten.Events.Daemon.Resiliency;
using Marten.Events.Projections;
using Stripe;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationInsightsTelemetry();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Logging.ClearProviders();

builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssemblyContaining<Program>();
});

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

StripeConfiguration.ApiKey = Environment.GetEnvironmentVariable("STRIPE_SECRET_KEY") ?? builder.Configuration["Stripe:SecretKey"];

builder.Services.AddScoped<IChargePointRepository, ChargePointRepository>();
builder.Services.AddScoped<IChargeSessionRepository, ChargeSessionRepository>();
builder.Services.AddScoped<IPricingGroupRepository, PricingGroupRepository>();
builder.Services.AddScoped<ISessionBillingRepository, SessionBillingRepository>();
builder.Services.AddScoped<IStripeService, StripeService>();

builder.Services.AddMarten(options => {
        options.Connection(builder.Configuration.GetConnectionString("MartenDb") ?? string.Empty);
        
        options.UseNewtonsoftForSerialization();
        
        options.Projections.Add<ChargePointProjection>(ProjectionLifecycle.Inline);
        options.Projections.Add<ChargeSessionProjection>(ProjectionLifecycle.Inline);
        options.Projections.Add<ChargeTagProjection>(ProjectionLifecycle.Inline);
        options.Projections.Add<LocationProjection>(ProjectionLifecycle.Inline);
        options.Projections.Add<PricingGroupProjection>(ProjectionLifecycle.Inline);
        options.Projections.Add<SessionBillingProjection>(ProjectionLifecycle.Inline);
        
        options.Schema.For<LocationReadModel>().Identity(x => x.Id);
        options.Schema.For<ChargePointReadModel>().Identity(x => x.Id);
        options.Schema.For<ChargeSessionReadModel>().Identity(x => x.Id);
        options.Schema.For<ChargeTagReadModel>().Identity(x => x.Id);
        options.Schema.For<ConnectorReadModel>().Identity(x => x.ConnectorId);
        options.Schema.For<ConnectorErrorReadModel>().Identity(x => new { x.Id});
        options.Schema.For<PricingGroupReadModel>().Identity(x => x.Id);
        options.Schema.For<SessionBillingReadModel>().Identity(x => x.Id);
    })
    .UseLightweightSessions()
    .AddAsyncDaemon(DaemonMode.Solo);

builder.Services.AddSingleton<ILoggerService, LoggerService>();
builder.Services.AddScoped<IChargePointRepository, ChargePointRepository>();
builder.Services.AddScoped<IChargeSessionRepository, ChargeSessionRepository>();
builder.Services.AddScoped<ILocationRepository, LocationRepository>();

builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("MartenDb") ?? string.Empty);

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c => {
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "CPMS API v1");
    c.RoutePrefix = "swagger";
});

app.UseRouting();
app.UseHttpsRedirection();
app.UseAuthorization();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();
app.MapHealthChecks("/health");
app.MapFallbackToFile("index.html");

await app.RunAsync();