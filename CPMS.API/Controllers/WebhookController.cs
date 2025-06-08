using CPMS.API.Projections;
using CPMS.API.Repositories;
using Marten;
using Microsoft.AspNetCore.Mvc;
using Stripe;

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
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        var endpointSecret = Environment.GetEnvironmentVariable("STRIPE_WEBHOOK_SECRET_PROD") ?? _configuration["Stripe:WebhookSecretProd"];
        
        try
        {
            var stripeEvent = EventUtility.ConstructEvent(json, Request.Headers["Stripe-Signature"], endpointSecret);
        
            _logger.LogInformation("Stripe webhook: {EventType}", stripeEvent.Type);

            if (stripeEvent.Type == "checkout.session.completed")
            {
                var session = stripeEvent.Data.Object as Stripe.Checkout.Session;
            
                var billingReadModel = await _querySession
                    .Query<SessionBillingReadModel>()
                    .FirstOrDefaultAsync(b => b.StripeSessionId == session.Id);

                if (billingReadModel != null)
                {
                    var billing = await _billingRepository.GetByIdAsync(billingReadModel.Id);
                    billing?.MarkAsPaid();
                    await _billingRepository.UpdateAsync(billing);
                }
            }

            return Ok();
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe webhook error");
            return BadRequest();
        }
    }
}