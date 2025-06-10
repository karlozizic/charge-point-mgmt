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
    private readonly ISessionBillingRepository _billingRepository;

    public WebhookController(
        IConfiguration configuration,
        ILogger<WebhookController> logger,
        IQuerySession querySession,
        ISessionBillingRepository billingRepository)
    {
        _configuration = configuration;
        _logger = logger;
        _querySession = querySession;
        _billingRepository = billingRepository;
    }

    [HttpPost("stripe")]
    public async Task<IActionResult> HandleStripeWebhook()
    {
        _logger.LogInformation("Received Stripe webhook request");
        
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        var endpointSecret = Environment.GetEnvironmentVariable("STRIPE_WEBHOOK_SECRET_PROD") ?? 
                             _configuration["Stripe:WebhookSecretProd"];

        try
        {
            var stripeEvent = EventUtility.ConstructEvent(json, Request.Headers["Stripe-Signature"], endpointSecret);
        
            _logger.LogInformation("Webhook: {EventType}", stripeEvent.Type);

            if (stripeEvent.Type == "checkout.session.completed")
            {
                if (stripeEvent.Data.Object is Session session)
                {
                    var billing = await _querySession
                        .Query<SessionBillingReadModel>()
                        .FirstOrDefaultAsync(b => b.StripeSessionId == session.Id);

                    if (billing != null)
                    {
                        await MarkBillingAsPaid(billing.Id);
                    }
                }
            }
            else if (stripeEvent.Type == "payment_intent.succeeded")
            {
                var recentBilling = await _querySession
                    .Query<SessionBillingReadModel>()
                    .Where(b => b.PaymentStatus == "pending_payment")
                    .OrderByDescending(b => b.CreatedAt)
                    .FirstOrDefaultAsync();

                if (recentBilling != null)
                {
                    _logger.LogInformation("Marking recent billing as paid: {BillingId}", recentBilling.Id);
                    await MarkBillingAsPaid(recentBilling.Id);
                }
            }

            return Ok();
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe webhook error");
            return BadRequest();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Webhook error");
            return BadRequest();
        }
    }

    private async Task MarkBillingAsPaid(Guid billingId)
    {
        try
        {
            var billing = await _billingRepository.GetByIdAsync(billingId);
            if (billing?.PaymentStatus != "succeeded")
            {
                billing?.MarkAsPaid();
                await _billingRepository.UpdateAsync(billing);
                _logger.LogInformation("Billing marked as paid: {BillingId}", billingId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to mark billing as paid: {BillingId}", billingId);
        }
    }
}