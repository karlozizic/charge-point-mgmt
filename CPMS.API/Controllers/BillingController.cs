using CPMS.API.Dtos;
using CPMS.API.Handlers.Billing;
using CPMS.API.Projections;
using Marten;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CPMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BillingController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IQuerySession _querySession;

    public BillingController(IMediator mediator, IQuerySession querySession)
    {
        _mediator = mediator;
        _querySession = querySession;
    }

    [HttpGet("sessions/{sessionId}")]
    public async Task<ActionResult<SessionBillingReadModel>> GetSessionBilling(Guid sessionId)
    {
        var billing = await _querySession
            .Query<SessionBillingReadModel>()
            .FirstOrDefaultAsync(b => b.SessionId == sessionId);
            
        if (billing == null)
            return NotFound();
            
        return Ok(billing);
    }

    [HttpGet("unpaid")]
    public async Task<ActionResult<List<SessionBillingReadModel>>> GetUnpaidBillings()
    {
        var unpaidBillings = await _querySession
            .Query<SessionBillingReadModel>()
            .Where(b => b.PaymentStatus != "succeeded")
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
            
        return Ok(unpaidBillings);
    }   
    
    [HttpPost("sessions/{sessionId}/checkout")]
    public async Task<ActionResult<CheckoutSessionResponse>> CreateCheckoutSession(
        Guid sessionId,
        [FromBody] CreateCheckoutRequest request)
    {
        var billing = await _querySession
            .Query<SessionBillingReadModel>()
            .FirstOrDefaultAsync(b => b.SessionId == sessionId);

        if (billing == null)
            return NotFound("Billing record not found");

        var command = new CreateCheckoutSessionCommand
        {
            SessionBillingId = billing.Id,
            CustomerEmail = request.CustomerEmail
        };

        var response = await _mediator.Send(command);
        return Ok(response);
    }
}