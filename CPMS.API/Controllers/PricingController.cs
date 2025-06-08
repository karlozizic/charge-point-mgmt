using CPMS.API.Handlers.PricingGroup;
using CPMS.API.Projections;
using Marten;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CPMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PricingController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IQuerySession _querySession;

    public PricingController(IMediator mediator, IQuerySession querySession)
    {
        _mediator = mediator;
        _querySession = querySession;
    }

    [HttpGet("groups")]
    public async Task<ActionResult<List<PricingGroupReadModel>>> GetPricingGroups()
    {
        var groups = await _querySession
            .Query<PricingGroupReadModel>()
            .Where(pg => pg.IsActive)
            .ToListAsync();
            
        return Ok(groups);
    }

    [HttpPost("groups")]
    public async Task<ActionResult<Guid>> CreatePricingGroup(CreatePricingGroupCommand command)
    {
        var id = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetPricingGroup), new { id }, id);
    }

    [HttpGet("groups/{id}")]
    public async Task<ActionResult<PricingGroupReadModel>> GetPricingGroup(Guid id)
    {
        var group = await _querySession
            .Query<PricingGroupReadModel>()
            .FirstOrDefaultAsync(pg => pg.Id == id);
            
        if (group == null)
            return NotFound();
            
        return Ok(group);
    }

    [HttpPost("groups/{id}/assign-chargepoint/{chargePointId}")]
    public async Task<ActionResult> AssignChargePoint(Guid id, Guid chargePointId)
    {
        await _mediator.Send(new AssignChargePointToPricingGroupCommand
        { 
            PricingGroupId = id, 
            ChargePointId = chargePointId 
        });
        
        return NoContent();
    }
}