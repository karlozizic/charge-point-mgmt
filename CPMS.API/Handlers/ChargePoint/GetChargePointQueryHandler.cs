using CPMS.API.Dtos;
using CPMS.API.Projections;
using Marten;
using MediatR;

namespace CPMS.API.Handlers.ChargePoint;

public class GetChargePointQuery : IRequest<ChargePointDto>
{
    public Guid Id { get; set; }
}

public class GetChargePointQueryHandler : IRequestHandler<GetChargePointQuery, ChargePointDto>
{
    private readonly IQuerySession _querySession;

    public GetChargePointQueryHandler(IQuerySession querySession)
    {
        _querySession = querySession;
    }

    public async Task<ChargePointDto> Handle(GetChargePointQuery query, CancellationToken cancellationToken)
    {
        var readModel = await _querySession
            .Query<ChargePointReadModel>()
            .SingleOrDefaultAsync(cp => cp.Id == query.Id, cancellationToken);

        if (readModel == null)
            return null;

        return new ChargePointDto
        {
            Id = readModel.Id,
            Name = readModel.Name,
            LocationId = readModel.LocationId,
            MaxPower = readModel.MaxPower,
            CurrentPower = readModel.CurrentPower,
            Connectors = readModel.Connectors?.Select(c => new ConnectorDto
            {
                Id = c.ConnectorId,
                Name = c.Name,
                Status = c.Status,
                LastStatusTime = c.LastStatusTime
            }).ToList()
        };
    }
}