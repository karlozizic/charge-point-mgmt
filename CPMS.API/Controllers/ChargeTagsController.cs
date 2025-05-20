using CPMS.API.Handlers.ChargeSession;
using CPMS.API.Handlers.ChargeTag;
using CPMS.API.Projections;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CPMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChargeTagsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ChargeTagsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<ChargeTagReadModel>>> GetAll()
    {
        var tags = await _mediator.Send(new GetAllChargeTagsQuery());
        return Ok(tags);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ChargeTagReadModel>> GetById(Guid id)
    {
        var tag = await _mediator.Send(new GetChargeTagByIdQuery { Id = id });
        
        if (tag == null)
            return NotFound();
            
        return Ok(tag);
    }

    [HttpGet("byTagId/{tagId}")]
    public async Task<ActionResult<ChargeTagReadModel>> GetByTagId(string tagId)
    {
        var tag = await _mediator.Send(new GetChargeTagByTagIdQuery { TagId = tagId });
        
        if (tag == null)
            return NotFound();
            
        return Ok(tag);
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(CreateChargeTagCommand command)
    {
        try
        {
            var id = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id }, id);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(Guid id, UpdateChargeTagCommand command)
    {
        try
        {
            command.Id = id;
            await _mediator.Send(command);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost("{id}/block")]
    public async Task<ActionResult> Block(Guid id)
    {
        try
        {
            await _mediator.Send(new BlockChargeTagCommand { Id = id });
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost("{id}/unblock")]
    public async Task<ActionResult> Unblock(Guid id)
    {
        try
        {
            await _mediator.Send(new UnblockChargeTagCommand { Id = id });
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPut("{id}/expiry")]
    public async Task<ActionResult> UpdateExpiry(Guid id, UpdateChargeTagExpiryCommand command)
    {
        try
        {
            command.Id = id;
            await _mediator.Send(command);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }
    
    [HttpPost("authorize")]
    public async Task<ActionResult<bool>> AuthorizeTag(AuthorizeTagCommand command)
    {
        var isAuthorized = await _mediator.Send(command);
        return Ok(isAuthorized);
    }
}