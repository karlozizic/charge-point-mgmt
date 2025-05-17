using CPMS.API.Handlers;
using CPMS.API.Handlers.ChargePoint;
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

builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssemblyContaining<Program>();
});

builder.Services.AddMarten(options => {
        options.Connection(builder.Configuration.GetConnectionString("MartenDb") ?? string.Empty);
        
        options.UseNewtonsoftForSerialization();
        
        options.Projections.Add<ChargePointProjection>(ProjectionLifecycle.Inline);
        options.Projections.Add<ChargeSessionProjection>(ProjectionLifecycle.Inline);
        options.Projections.Add<ChargeTagProjection>(ProjectionLifecycle.Inline);
    })
    .UseLightweightSessions()
    .AddAsyncDaemon(DaemonMode.Solo);

builder.Services.AddSingleton<ILoggerService, LoggerService>();
builder.Services.AddScoped<IChargePointRepository, ChargePointRepository>();
builder.Services.AddScoped<IChargeSessionRepository, ChargeSessionRepository>();

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

app.MapFallbackToFile("index.html");

await app.RunAsync();