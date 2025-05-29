using CPMS.API.Projections;
using CPMS.API.Repositories;
using CPMS.BuildingBlocks.Infrastructure.Logger;
using Marten;
using Marten.Events.Daemon.Resiliency;
using Marten.Events.Projections;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssemblyContaining<Program>();
});

builder.Services.AddSingleton<ILoggerService, LoggerService>();
builder.Services.AddScoped<IChargePointRepository, ChargePointRepository>();
builder.Services.AddScoped<IChargeSessionRepository, ChargeSessionRepository>();

builder.Services.AddMarten(options => {
        options.Connection(builder.Configuration.GetConnectionString("MartenDb") ?? string.Empty);
        
        options.UseNewtonsoftForSerialization();
        
        options.Projections.Add<ChargePointProjection>(ProjectionLifecycle.Inline);
        options.Projections.Add<ChargeSessionProjection>(ProjectionLifecycle.Inline);
        options.Projections.Add<ChargeTagProjection>(ProjectionLifecycle.Inline);
        options.Projections.Add<LocationProjection>(ProjectionLifecycle.Inline);
        
        options.Schema.For<LocationReadModel>().Identity(x => x.Id);
        options.Schema.For<ChargePointReadModel>().Identity(x => x.Id);
        options.Schema.For<ChargeSessionReadModel>().Identity(x => x.Id);
        options.Schema.For<ChargeTagReadModel>().Identity(x => x.Id);
        options.Schema.For<ConnectorReadModel>().Identity(x => x.ConnectorId);
        options.Schema.For<ConnectorErrorReadModel>().Identity(x => new { x.ConnectorId, x.Timestamp });
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

//if (app.Environment.IsDevelopment())
//{
app.UseSwagger();
app.UseSwaggerUI(c => {
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "CPMS API v1");
    c.RoutePrefix = "swagger";
});
//}

app.UseRouting();
app.UseHttpsRedirection();
app.UseAuthorization();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();
app.MapHealthChecks("/health");
app.MapFallbackToFile("index.html");

await app.RunAsync();