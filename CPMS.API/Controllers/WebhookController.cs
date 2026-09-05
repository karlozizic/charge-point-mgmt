using CPMS.API.Projections;
using CPMS.API.Repositories;
using Marten;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;

namespace CPMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WebhookController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<WebhookController> _logger;
    private readonly IQuerySession _querySession;
    private readonly IAggregateRepository<Entities.SessionBilling> _billings;

    public WebhookController(
        IConfiguration configuration,
        ILogger<WebhookController> logger,
        IQuerySession querySession,
        IAggregateRepository<Entities.SessionBilling> billings)
    {
        _configuration = configuration;
        _logger = logger;
        _querySession = querySession;
        _billings = billings;
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

        _logger.LogInformation("Stripe webhook: {EventType}", stripeEvent.Type);

        switch (stripeEvent.Type)
        {
            case "checkout.session.completed" when stripeEvent.Data.Object is Session session:
            {
                var billing = await _querySession
                    .Query<SessionBillingReadModel>()
                    .FirstOrDefaultAsync(b => b.StripeSessionId == session.Id);
                if (billing != null)
                    await MarkBillingAsPaid(billing.Id);
                break;
            }
            case "payment_intent.succeeded":
            {
                // Known gap: the payment intent is not correlated to a billing, so the most recent
                // pending one is assumed. Can mark the wrong session as paid.
                var recentBilling = await _querySession
                    .Query<SessionBillingReadModel>()
                    .Where(b => b.PaymentStatus == "pending_payment")
                    .OrderByDescending(b => b.CreatedAt)
                    .FirstOrDefaultAsync();
                if (recentBilling != null)
                    await MarkBillingAsPaid(recentBilling.Id);
                break;
            }
        }

        return Ok();
    }

    private async Task MarkBillingAsPaid(Guid billingId)
    {
        var billing = await _billings.LoadAsync(billingId);
        if (billing == null)
            return;

        billing.MarkAsPaid();
        await _billings.SaveAsync(billing);
        _logger.LogInformation("Billing marked as paid: {BillingId}", billingId);
    }
}
