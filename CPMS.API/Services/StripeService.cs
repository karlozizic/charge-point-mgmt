using CPMS.API.Dtos;
using Stripe;
using Stripe.Checkout;

namespace CPMS.API.Services;

public class StripeService : IStripeService
{
    private readonly SessionService _sessionService;
    private readonly ILogger<StripeService> _logger;

    public StripeService(ILogger<StripeService> logger)
    {
        _sessionService = new SessionService();
        _logger = logger;
    }

    public async Task<CheckoutSessionResponse> CreateCheckoutSessionAsync(
        decimal amount,
        string currency,
        string description,
        string customerEmail,
        string successUrl,
        string cancelUrl)
    {
        try
        {
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new()
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(amount * 100),
                            Currency = currency.ToLower(),
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = "Charging Session",
                                Description = description,
                            },
                        },
                        Quantity = 1,
                    },
                },
                Mode = "payment",
                CustomerEmail = customerEmail,
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl,
                Metadata = new Dictionary<string, string>
                {
                    { "source", "cpms" },
                    { "description", description }
                }
            };

            var session = await _sessionService.CreateAsync(options);
            
            _logger.LogInformation("Created Stripe Checkout Session {SessionId} for amount {Amount} {Currency}",
                session.Id, amount, currency);

            return new CheckoutSessionResponse
            {
                SessionId = session.Id,
                Url = session.Url
            };
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Failed to create Stripe Checkout Session");
            throw new InvalidOperationException("Failed to create checkout session", ex);
        }
    }
}

public interface IStripeService
{
    Task<CheckoutSessionResponse> CreateCheckoutSessionAsync(
        decimal amount,
        string currency,
        string description,
        string customerEmail,
        string successUrl,
        string cancelUrl);
}