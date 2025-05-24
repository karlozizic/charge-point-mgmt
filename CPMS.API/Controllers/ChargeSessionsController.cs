using CPMS.API.Dtos;
using CPMS.API.Handlers.ChargeSession;
using CPMS.API.Projections;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CPMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChargeSessionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ChargeSessionsController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpGet]
    public async Task<ActionResult<List<ChargeSessionReadModel>>> GetAll(
        [FromQuery] int? pageNumber = null,
        [FromQuery] int? pageSize = null,
        [FromQuery] SessionStatus? status = null,
        [FromQuery] string? tagId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var query = new GetAllChargeSessionsQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            Status = status,
            TagId = tagId,
            StartDate = startDate,
            EndDate = endDate
        };

        var sessions = await _mediator.Send(query);
        
        return Ok(sessions);
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<ChargeSessionReadModel>> GetById(Guid id)
    {
        var session = await _mediator.Send(new GetChargeSessionByIdQuery { SessionId = id });
            
        if (session == null)
            return NotFound();
                
        return Ok(session);
    }
    
    [HttpGet("active")]
    public async Task<ActionResult<List<ChargeSessionReadModel>>> GetActive()
    {
        var sessions = await _mediator.Send(new GetActiveChargeSessionsQuery());
        return Ok(sessions);
    }
    
    [HttpGet("by-chargepoint/{chargePointId}")]
    public async Task<ActionResult<List<ChargeSessionReadModel>>> GetByChargePoint(
        string chargePointId,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        var query = new GetChargeSessionsByChargePointQuery
        {
            ChargePointId = chargePointId,
            FromDate = fromDate,
            ToDate = toDate
        };

        var sessions = await _mediator.Send(query);
        return Ok(sessions);
    }
    
    [HttpGet("stats")]
    public async Task<ActionResult<ChargeSessionStatsDto>> GetStats(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? chargePointId = null)
    {
        var query = new GetChargeSessionStatsQuery
        {
            FromDate = fromDate,
            ToDate = toDate,
            ChargePointId = chargePointId
        };

        var stats = await _mediator.Send(query);
        return Ok(stats);
    }
}