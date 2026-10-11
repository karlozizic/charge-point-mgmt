using CPMS.API.Repositories;
using MediatR;

namespace CPMS.API.Handlers.Billing;

public record MarkBillingPaidCommand(Guid BillingId, string? PaymentIntentId) : IRequest;

public class MarkBillingPaidCommandHandler : IRequestHandler<MarkBillingPaidCommand>
{
    private readonly IAggregateRepository<Entities.SessionBilling> _billings;
    private readonly ILogger<MarkBillingPaidCommandHandler> _logger;

    public MarkBillingPaidCommandHandler(
        IAggregateRepository<Entities.SessionBilling> billings,
        ILogger<MarkBillingPaidCommandHandler> logger)
    {
        _billings = billings;
        _logger = logger;
    }

    public async Task Handle(MarkBillingPaidCommand command, CancellationToken cancellationToken)
    {
        var billing = await _billings.FindAsync(command.BillingId, cancellationToken);
        if (billing == null)
        {
            _logger.LogWarning("Payment received for unknown billing {BillingId}", command.BillingId);
            return;
        }

        billing.MarkAsPaid(command.PaymentIntentId);
        await _billings.UpdateAsync(billing, cancellationToken);
    }
}
