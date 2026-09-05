using CPMS.API.Projections;
using CPMS.API.Repositories;
using Marten;
using MediatR;

namespace CPMS.API.Handlers.ChargeTag;

public class CreateChargeTagCommand : IRequest<Guid>
{
    public string TagId { get; set; }
    public DateTime? ExpiryDate { get; set; }
}

public class CreateChargeTagCommandHandler : IRequestHandler<CreateChargeTagCommand, Guid>
{
    private readonly IAggregateRepository<Entities.ChargeTag> _chargeTags;
    private readonly IQuerySession _querySession;

    public CreateChargeTagCommandHandler(
        IAggregateRepository<Entities.ChargeTag> chargeTags,
        IQuerySession querySession)
    {
        _chargeTags = chargeTags;
        _querySession = querySession;
    }

    public async Task<Guid> Handle(CreateChargeTagCommand request, CancellationToken cancellationToken)
    {
        var exists = await _querySession
            .Query<ChargeTagReadModel>()
            .AnyAsync(t => t.TagId == request.TagId, cancellationToken);
        if (exists)
            throw new InvalidOperationException($"Tag with ID {request.TagId} already exists.");

        var chargeTag = new Entities.ChargeTag(Guid.NewGuid(), request.TagId, request.ExpiryDate);

        await _chargeTags.SaveAsync(chargeTag, cancellationToken);
        return chargeTag.Id;
    }
}
