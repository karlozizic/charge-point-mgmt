using CPMS.API.Dtos;
using CPMS.API.Exceptions;
using CPMS.API.Handlers.Location;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CPMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LocationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public LocationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<LocationSummaryDto>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? city = null,
        [FromQuery] string? country = null
        )
    {
        var query = new GetAllLocationsQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            City = city,
            Country = country
        };
        
        var locations = await _mediator.Send(query);
        return Ok(locations);
    }
    
    [HttpPost]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateLocationCommand command)
    {
        var locationId = await _mediator.Send(command);
        
        return CreatedAtAction(nameof(GetById), 
            new { id = locationId }, locationId);
    }
    
    [HttpPut]
    public async Task<ActionResult> Update([FromBody] UpdateLocationCommand command)
    {
        try
        {
            await _mediator.Send(command);
            return NoContent();
        }
        catch (NotFoundException)
        {
            return NotFound(new { Message = "Location not found." });
        }
        catch (InvalidOperationException)
        {
            return BadRequest(new { Message = "Invalid operation." });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<LocationDto>> GetById(Guid id)
    {
        var location = await _mediator.Send(new GetLocationQuery { Id = id });
        
        if (location == null)
            return NotFound();
            
        return Ok(location);
    }
}