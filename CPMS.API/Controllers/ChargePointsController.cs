using CPMS.API.Dtos;
using CPMS.API.Handlers;
using CPMS.API.Handlers.ChargePoint;
using CPMS.API.Handlers.Connector;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CPMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChargePointsController : ControllerBase
{
    private readonly IMediator _mediator;
        
    public ChargePointsController(IMediator mediator)
    {
        _mediator = mediator;
    }
        
    [HttpGet]
    public async Task<ActionResult<List<ChargePointSummaryDto>>> GetAll()
    {
        var result = await _mediator.Send(new GetAllChargePointsQuery());
        return Ok(result);
    }
        
    [HttpGet("{id}")]
    public async Task<ActionResult<ChargePointDto>> Get(Guid id)
    {
        var result = await _mediator.Send(new GetChargePointQuery { Id = id });
                
        return Ok(result);
    }
        
    [HttpPost]
    public async Task<ActionResult<Guid>> Create(CreateChargePointCommand command)
    {
        var id = await _mediator.Send(command);
        return CreatedAtAction(nameof(Get), new { id }, id);
    }
        
    [HttpPost("{id}/connectors")]
    public async Task<ActionResult> AddConnector(Guid id, AddConnectorCommand command)
    {
        command.ChargePointId = id;
        await _mediator.Send(command);
        return NoContent();
    }
        
    [HttpPut("{id}/connectors/{connectorId}/status")]
    public async Task<ActionResult> UpdateConnectorStatus(
        Guid id, 
        int connectorId, 
        UpdateConnectorStatusCommand command)
    {
        command.ChargePointId = id;
        command.ConnectorId = connectorId;
        await _mediator.Send(command);
        return NoContent();
    }
}