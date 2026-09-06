using CPMS.API.Projections;
using JasperFx.Events.Projections;
using Marten;

namespace CPMS.API.Infrastructure;

/// <summary>
/// The one place that says how Marten is configured. Program.cs and the integration tests both
/// call it, so a test store cannot drift from the store the application really uses.
/// </summary>
public static class MartenConfiguration
{
    public static void Configure(StoreOptions options, string connectionString)
    {
        options.Connection(connectionString);
        options.UseNewtonsoftForSerialization();

        // All read models are written in the same transaction as the events.
        options.Projections.Add<ChargePointProjection>(ProjectionLifecycle.Inline);
        options.Projections.Add<ChargeSessionProjection>(ProjectionLifecycle.Inline);
        options.Projections.Add<ChargeTagProjection>(ProjectionLifecycle.Inline);
        options.Projections.Add<LocationProjection>(ProjectionLifecycle.Inline);
        options.Projections.Add<PricingGroupProjection>(ProjectionLifecycle.Inline);
        options.Projections.Add<SessionBillingProjection>(ProjectionLifecycle.Inline);
    }
}
