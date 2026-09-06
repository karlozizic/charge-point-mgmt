using CPMS.API.Infrastructure;
using CPMS.API.Repositories;
using CPMS.API.Services;
using CPMS.BuildingBlocks.Infrastructure.Logger;
using Marten;
using Stripe;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();

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

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());

StripeConfiguration.ApiKey = Environment.GetEnvironmentVariable("STRIPE_SECRET_KEY") ?? builder.Configuration["Stripe:SecretKey"];

var connectionString = builder.Configuration.GetConnectionString("MartenDb")
    ?? throw new InvalidOperationException("ConnectionStrings:MartenDb is not configured.");

builder.Services.AddMarten(options => MartenConfiguration.Configure(options, connectionString))
    .UseLightweightSessions();

builder.Services.AddSingleton<ILoggerService, LoggerService>();
builder.Services.AddScoped(typeof(IAggregateRepository<>), typeof(MartenAggregateRepository<>));
builder.Services.AddScoped<IStripeService, StripeService>();

builder.Services.AddHealthChecks().AddNpgSql(connectionString);

var app = builder.Build();

app.UseExceptionHandler();

app.UseSwagger();
app.UseSwaggerUI(c => {
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "CPMS API v1");
    c.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();

// Serves the operator console when it is built into wwwroot (npm run build:dotnet).
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();
app.MapHealthChecks("/health");
app.MapFallbackToFile("index.html");

await app.RunAsync();
