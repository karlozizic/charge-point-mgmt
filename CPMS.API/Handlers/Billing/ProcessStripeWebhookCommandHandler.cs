using CPMS.API.Projections;
using CPMS.API.Repositories;
using Marten;
using MediatR;

namespace CPMS.API.Handlers.Billing;

public class ProcessStripeWebhookCommand : IRequest
{
    public string EventType { get; set; }
    public string PaymentIntentId { get; set; }
    public string Status { get; set; }
}

public class ProcessStripeWebhookCommandHandler : IRequestHandler<ProcessStripeWebhookCommand>
{
    private readonly ISessionBillingRepository _repository;
    private readonly IQuerySession _querySession;
    
    public ProcessStripeWebhookCommandHandler(
        ISessionBillingRepository repository,
        IQuerySession querySession)
    {
        _repository = repository;
        _querySession = querySession;
    }
    
    public async Task Handle(ProcessStripeWebhookCommand command, CancellationToken cancellationToken)
    {
        if (command.EventType != "payment_intent.succeeded")
            return;
        
        var billingReadModel = await _querySession
            .Query<SessionBillingReadModel>()
            .FirstOrDefaultAsync(b => b.StripePaymentIntentId == command.PaymentIntentId, 
                cancellationToken);
        
        if (billingReadModel == null)
            return;
        
        var billing = await _repository.GetByIdAsync(billingReadModel.Id);
        if (billing != null)
        {
            billing.MarkAsPaid();
            await _repository.UpdateAsync(billing);
        }
    }
}