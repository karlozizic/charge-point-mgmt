using System.Text;
using CPMS.API.Controllers;
using CPMS.API.Entities;
using CPMS.API.Events.Billing;
using CPMS.API.Handlers.Billing;
using CPMS.API.Projections;
using CPMS.API.Repositories;
using Marten;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Stripe;

namespace CPMS.IntegrationTests;

[Collection(nameof(PostgresCollection))]
public class StripeWebhookTests
{
    private const string Secret = "whsec_test_secret";

    private readonly PostgresFixture _fixture;

    public StripeWebhookTests(PostgresFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task MarkBillingPaid_marks_only_the_billing_in_the_command()
    {
        await using var services = _fixture.NewServices();
        var first = await SeedBillingAsync(services);
        var second = await SeedBillingAsync(services);

        await SendAsync(services, new MarkBillingPaidCommand(first, "pi_1"));

        var billings = await LoadAsync(services, first, second);
        Assert.Equal("succeeded", billings[first].PaymentStatus);
        Assert.Equal("pi_1", billings[first].StripePaymentIntentId);
        Assert.Equal("pending", billings[second].PaymentStatus);
    }

    [Fact]
    public async Task MarkBillingPaid_twice_records_one_payment()
    {
        await using var services = _fixture.NewServices();
        var billingId = await SeedBillingAsync(services);

        await SendAsync(services, new MarkBillingPaidCommand(billingId, "pi_1"));
        await SendAsync(services, new MarkBillingPaidCommand(billingId, "pi_1"));

        Assert.Equal(1, await CountPaymentsAsync(services));
    }

    [Fact]
    public async Task MarkBillingPaid_for_an_unknown_billing_changes_nothing()
    {
        await using var services = _fixture.NewServices();
        await SeedBillingAsync(services);

        await SendAsync(services, new MarkBillingPaidCommand(Guid.NewGuid(), "pi_1"));

        Assert.Equal(0, await CountPaymentsAsync(services));
    }

    [Fact]
    public async Task Webhook_with_a_signed_payment_intent_marks_the_billing_in_its_metadata_paid()
    {
        await using var services = _fixture.NewServices();
        var billingId = await SeedBillingAsync(services);
        var payload = PaymentIntentSucceeded("pi_42", billingId.ToString());

        var result = await PostAsync(services, payload, Sign(payload));

        Assert.IsType<OkResult>(result);
        var billing = (await LoadAsync(services, billingId))[billingId];
        Assert.Equal("succeeded", billing.PaymentStatus);
        Assert.Equal("pi_42", billing.StripePaymentIntentId);
    }

    [Fact]
    public async Task Webhook_without_billing_metadata_is_acknowledged_and_changes_nothing()
    {
        await using var services = _fixture.NewServices();
        await SeedBillingAsync(services);
        var payload = PaymentIntentSucceeded("pi_42", billingId: null);

        var result = await PostAsync(services, payload, Sign(payload));

        Assert.IsType<OkResult>(result);
        Assert.Equal(0, await CountPaymentsAsync(services));
    }

    [Fact]
    public async Task Webhook_with_a_bad_signature_is_rejected()
    {
        await using var services = _fixture.NewServices();
        var billingId = await SeedBillingAsync(services);
        var payload = PaymentIntentSucceeded("pi_42", billingId.ToString());

        var result = await PostAsync(services, payload, "t=1,v1=bad");

        Assert.IsType<BadRequestResult>(result);
        Assert.Equal(0, await CountPaymentsAsync(services));
    }

    private static string PaymentIntentSucceeded(string paymentIntentId, string? billingId)
    {
        var metadata = billingId == null ? "{}" : $"{{\"{WebhookController.BillingIdMetadataKey}\":\"{billingId}\"}}";
        return $$"""
            {"id":"evt_1","object":"event","api_version":"{{StripeConfiguration.ApiVersion}}",
             "type":"payment_intent.succeeded",
             "data":{"object":{"id":"{{paymentIntentId}}","object":"payment_intent","metadata":{{metadata}} } } }
            """;
    }

    private static string Sign(string payload)
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        return $"t={timestamp},v1={EventUtility.ComputeSignature(Secret, timestamp, payload)}";
    }

    private static async Task<IActionResult> PostAsync(IServiceProvider services, string payload, string signature)
    {
        await using var scope = services.CreateAsyncScope();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Stripe:WebhookSecretProd"] = Secret })
            .Build();
        var context = new DefaultHttpContext();
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(payload));
        context.Request.Headers["Stripe-Signature"] = signature;

        var controller = new WebhookController(
            configuration,
            NullLogger<WebhookController>.Instance,
            scope.ServiceProvider.GetRequiredService<IMediator>())
        {
            ControllerContext = new ControllerContext { HttpContext = context }
        };

        return await controller.HandleStripeWebhook();
    }

    private static async Task<Guid> SeedBillingAsync(IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var billing = new SessionBilling(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 1m, 2m, "EUR");
        await scope.ServiceProvider.GetRequiredService<IAggregateRepository<SessionBilling>>().AddAsync(billing);
        return billing.Id;
    }

    private static async Task SendAsync(IServiceProvider services, MarkBillingPaidCommand command)
    {
        await using var scope = services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<IMediator>().Send(command);
    }

    private static async Task<Dictionary<Guid, SessionBillingReadModel>> LoadAsync(IServiceProvider services, params Guid[] ids)
    {
        await using var scope = services.CreateAsyncScope();
        var query = scope.ServiceProvider.GetRequiredService<IQuerySession>();
        var billings = await query.LoadManyAsync<SessionBillingReadModel>(ids);
        return billings.ToDictionary(b => b.Id);
    }

    private static async Task<int> CountPaymentsAsync(IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<IQuerySession>()
            .Events.QueryRawEventDataOnly<PaymentCompletedEvent>().CountAsync();
    }
}
