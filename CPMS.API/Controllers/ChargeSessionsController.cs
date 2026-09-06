using CPMS.API.Dtos;
using CPMS.API.Handlers.ChargeSession;
using CPMS.API.Projections;
using CPMS.Core.Common;
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
    public async Task<ActionResult<PagedResult<ChargeSessionReadModel>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] SessionStatus? status = null,
        [FromQuery] string? tagId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var sessions = await _mediator.Send(new GetAllChargeSessionsQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            Status = status,
            TagId = tagId,
            StartDate = startDate,
            EndDate = endDate
        });

        return Ok(sessions);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ChargeSessionReadModel>> GetById(Guid id)
    {
        var session = await _mediator.Send(new GetChargeSessionByIdQuery { SessionId = id });
        return session == null ? NotFound() : Ok(session);
    }

    [HttpGet("active")]
    public async Task<ActionResult<List<ChargeSessionReadModel>>> GetActive()
    {
        return Ok(await _mediator.Send(new GetActiveChargeSessionsQuery()));
    }

    [HttpGet("by-chargepoint/{chargePointId}")]
    public async Task<ActionResult<List<ChargeSessionReadModel>>> GetByChargePoint(
        string chargePointId,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        var sessions = await _mediator.Send(new GetChargeSessionsByChargePointQuery
        {
            ChargePointId = chargePointId,
            FromDate = fromDate,
            ToDate = toDate
        });

        return Ok(sessions);
    }

    [HttpGet("stats")]
    public async Task<ActionResult<ChargeSessionStatsDto>> GetStats(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? chargePointId = null)
    {
        var stats = await _mediator.Send(new GetChargeSessionStatsQuery
        {
            FromDate = fromDate,
            ToDate = toDate,
            ChargePointId = chargePointId
        });

        return Ok(stats);
    }

    [HttpGet("{sessionId}/export/csv")]
    public async Task<IActionResult> ExportToCsv(Guid sessionId)
    {
        var csv = await _mediator.Send(new ExportChargeSessionToCsvQuery { SessionId = sessionId });
        var fileName = $"session-{sessionId}-{DateTime.Now:yyyyMMdd}.csv";

        return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", fileName);
    }
}
