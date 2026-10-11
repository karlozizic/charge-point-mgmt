using CPMS.API.Handlers.Billing;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;

namespace CPMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WebhookController : ControllerBase
{
    public const string BillingIdMetadataKey = "billingId";

    private readonly IConfiguration _configuration;
    private readonly ILogger<WebhookController> _logger;
    private readonly IMediator _mediator;

    public WebhookController(
        IConfiguration configuration,
        ILogger<WebhookController> logger,
        IMediator mediator)
    {
        _configuration = configuration;
        _logger = logger;
        _mediator = mediator;
    }

    [HttpPost("stripe")]
    public async Task<IActionResult> HandleStripeWebhook()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        var endpointSecret = Environment.GetEnvironmentVariable("STRIPE_WEBHOOK_SECRET_PROD") ??
                             _configuration["Stripe:WebhookSecretProd"];

        Event stripeEvent;
        try
        {
            stripeEvent = EventUtility.ConstructEvent(json, Request.Headers["Stripe-Signature"], endpointSecret);
        }
        catch (StripeException ex)
        {
            _logger.LogWarning(ex, "Rejected Stripe webhook: invalid payload or signature");
            return BadRequest();
        }

        _logger.LogInformation("Stripe webhook {EventId}: {EventType}", stripeEvent.Id, stripeEvent.Type);

        var payment = stripeEvent.Type switch
        {
            "checkout.session.completed" when stripeEvent.Data.Object is Session session =>
                (BillingId: BillingIdFrom(session.Metadata), PaymentIntentId: session.PaymentIntentId),
            "payment_intent.succeeded" when stripeEvent.Data.Object is PaymentIntent intent =>
                (BillingId: BillingIdFrom(intent.Metadata), PaymentIntentId: intent.Id),
            _ => (BillingId: null, PaymentIntentId: null)
        };

        if (payment.BillingId is { } billingId)
            await _mediator.Send(new MarkBillingPaidCommand(billingId, payment.PaymentIntentId));
        else if (stripeEvent.Type is "checkout.session.completed" or "payment_intent.succeeded")
            _logger.LogWarning("Stripe event {EventId} carries no {Key} metadata", stripeEvent.Id, BillingIdMetadataKey);

        return Ok();
    }

    private static Guid? BillingIdFrom(IDictionary<string, string>? metadata)
    {
        if (metadata == null || !metadata.TryGetValue(BillingIdMetadataKey, out var value))
            return null;

        return Guid.TryParse(value, out var billingId) ? billingId : null;
    }
}
