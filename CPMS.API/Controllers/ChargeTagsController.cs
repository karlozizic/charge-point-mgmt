using CPMS.API.Handlers.ChargeSession;
using CPMS.API.Handlers.ChargeTag;
using CPMS.API.Projections;
using CPMS.Core.Models.Responses;
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
        return Ok(await _mediator.Send(new GetAllChargeTagsQuery()));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ChargeTagReadModel>> GetById(Guid id)
    {
        var tag = await _mediator.Send(new GetChargeTagByIdQuery { Id = id });
        return tag == null ? NotFound() : Ok(tag);
    }

    [HttpGet("byTagId/{tagId}")]
    public async Task<ActionResult<ChargeTagReadModel>> GetByTagId(string tagId)
    {
        var tag = await _mediator.Send(new GetChargeTagByTagIdQuery { TagId = tagId });
        return tag == null ? NotFound() : Ok(tag);
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(CreateChargeTagCommand command)
    {
        var id = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(Guid id, UpdateChargeTagCommand command)
    {
        command.Id = id;
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpPost("{id}/block")]
    public async Task<ActionResult> Block(Guid id)
    {
        await _mediator.Send(new BlockChargeTagCommand { Id = id });
        return NoContent();
    }

    [HttpPost("{id}/unblock")]
    public async Task<ActionResult> Unblock(Guid id)
    {
        await _mediator.Send(new UnblockChargeTagCommand { Id = id });
        return NoContent();
    }

    [HttpPut("{id}/expiry")]
    public async Task<ActionResult> UpdateExpiry(Guid id, UpdateChargeTagExpiryCommand command)
    {
        command.Id = id;
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpPost("authorize")]
    public async Task<ActionResult<bool>> AuthorizeTag(AuthorizeTagCommand command)
    {
        // The console only needs yes/no; the granular status exists for the OCPP idTagInfo.
        return Ok(await _mediator.Send(command) == AuthorizationStatus.Accepted);
    }
}
