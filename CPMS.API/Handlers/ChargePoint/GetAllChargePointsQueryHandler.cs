using CPMS.API.Dtos;
using CPMS.API.Projections;
using Marten;
using MediatR;

namespace CPMS.API.Handlers.ChargePoint;

public class GetAllChargePointsQuery : IRequest<List<ChargePointSummaryDto>>
{
}
    
public class GetAllChargePointsQueryHandler : IRequestHandler<GetAllChargePointsQuery, List<ChargePointSummaryDto>>
{
    private readonly IQuerySession _querySession;
        
    public GetAllChargePointsQueryHandler(IQuerySession querySession)
    {
        _querySession = querySession;
    }
        
    public async Task<List<ChargePointSummaryDto>> Handle(GetAllChargePointsQuery query, CancellationToken cancellationToken)
    {
        var readModels = await _querySession
            .Query<ChargePointReadModel>()
            .ToListAsync(cancellationToken);
                
        return readModels.Select(rm => new ChargePointSummaryDto
        {
            Id = rm.Id,
            Name = rm.Name,
            LocationId = rm.LocationId,
            TotalConnectors = rm.Connectors?.Count ?? 0,
            MaxPower = rm.MaxPower,
            CurrentPower = rm.CurrentPower
        }).ToList();
    }
}