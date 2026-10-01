using CPMS.API.Repositories;
using Marten;

namespace CPMS.API.Infrastructure;

public static class ServiceRegistration
{
    public static IServiceCollection AddCpmsCore(
        this IServiceCollection services,
        string connectionString,
        Action<StoreOptions>? configureStore = null)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());

        services.AddMarten(options =>
        {
            MartenConfiguration.Configure(options, connectionString);
            configureStore?.Invoke(options);
        }).UseLightweightSessions();

        services.AddScoped(typeof(IAggregateRepository<>), typeof(MartenAggregateRepository<>));

        return services;
    }
}
