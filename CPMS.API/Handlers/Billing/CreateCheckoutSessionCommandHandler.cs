using CPMS.API.Dtos;
using CPMS.API.Exceptions;
using CPMS.API.Repositories;
using CPMS.API.Services;
using MediatR;

namespace CPMS.API.Handlers.Billing;

public class CreateCheckoutSessionCommand : IRequest<CheckoutSessionResponse>
{
    public Guid SessionBillingId { get; set; }
    public string CustomerEmail { get; set; }
}

public class CreateCheckoutSessionCommandHandler : IRequestHandler<CreateCheckoutSessionCommand, CheckoutSessionResponse>
{
    private readonly IAggregateRepository<Entities.SessionBilling> _billings;
    private readonly IStripeService _stripeService;
    private readonly IConfiguration _configuration;

    public CreateCheckoutSessionCommandHandler(
        IAggregateRepository<Entities.SessionBilling> billings,
        IStripeService stripeService,
        IConfiguration configuration)
    {
        _billings = billings;
        _stripeService = stripeService;
        _configuration = configuration;
    }

    public async Task<CheckoutSessionResponse> Handle(CreateCheckoutSessionCommand command, CancellationToken cancellationToken)
    {
        var billing = await _billings.LoadAsync(command.SessionBillingId, cancellationToken);
        if (billing == null)
            throw new NotFoundException("Billing record not found");

        var baseUrl = _configuration["App:FrontendUrl"];
        var successUrl = $"{baseUrl}/payment-success?session_id={{CHECKOUT_SESSION_ID}}&billing_session={billing.SessionId}";
        var cancelUrl = $"{baseUrl}/charge-sessions/{billing.SessionId}";

        var checkoutSession = await _stripeService.CreateCheckoutSessionAsync(
            billing.TotalAmount,
            billing.Currency,
            $"Charging Session - {billing.SessionId}",
            command.CustomerEmail,
            successUrl,
            cancelUrl);

        billing.SetStripeSessionId(checkoutSession.SessionId);
        await _billings.SaveAsync(billing, cancellationToken);

        return checkoutSession;
    }
}
