using CPMS.API.Handlers;
using CPMS.API.Handlers.ChargePoint;
using CPMS.API.Projections;
using CPMS.API.Repositories;
using Marten;
using Marten.Events.Daemon.Resiliency;
using Marten.Events.Projections;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(CreateChargePointCommandHandler).Assembly);
});

builder.Services.AddMarten(options => {
        options.Connection(builder.Configuration.GetConnectionString("MartenDb") ?? string.Empty);
        
        options.UseNewtonsoftForSerialization();
        options.Projections.Add<ChargePointProjection>(ProjectionLifecycle.Inline);
    })
    .UseLightweightSessions()
    .AddAsyncDaemon(DaemonMode.Solo);

builder.Services.AddScoped<IChargePointRepository, ChargePointRepository>();

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
app.MapControllers();

app.Run();