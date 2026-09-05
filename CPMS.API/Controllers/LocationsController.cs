using CPMS.API.Dtos;
using CPMS.API.Handlers.Location;
using CPMS.Core.Common;
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
    public async Task<ActionResult<PagedResult<LocationSummaryDto>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? city = null,
        [FromQuery] string? country = null)
    {
        var locations = await _mediator.Send(new GetAllLocationsQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            City = city,
            Country = country
        });

        return Ok(locations);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<LocationDto>> GetById(Guid id)
    {
        var location = await _mediator.Send(new GetLocationQuery { Id = id });
        return location == null ? NotFound() : Ok(location);
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateLocationCommand command)
    {
        var id = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpPut]
    public async Task<ActionResult> Update([FromBody] UpdateLocationCommand command)
    {
        await _mediator.Send(command);
        return NoContent();
    }
}
