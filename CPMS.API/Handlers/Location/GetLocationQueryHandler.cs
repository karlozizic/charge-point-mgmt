using CPMS.API.Dtos;
using CPMS.API.Projections;
using Marten;
using MediatR;

namespace CPMS.API.Handlers.Location;

public class GetLocationQuery : IRequest<LocationDto?>
{
    public Guid Id { get; set; }
}

public class GetLocationQueryHandler : IRequestHandler<GetLocationQuery, LocationDto?>
{
    private readonly IQuerySession _querySession;

    public GetLocationQueryHandler(IQuerySession querySession)
    {
        _querySession = querySession;
    }

    public async Task<LocationDto?> Handle(GetLocationQuery query, CancellationToken cancellationToken)
    {
        var readModel = await _querySession
            .Query<LocationReadModel>()
            .SingleOrDefaultAsync(l => l.Id == query.Id, cancellationToken);

        if (readModel == null)
            return null;

        var chargePoints = await _querySession
            .Query<ChargePointReadModel>()
            .Where(cp => cp.LocationId == query.Id)
            .ToListAsync(cancellationToken);

        return new LocationDto
        {
            Id = readModel.Id,
            Name = readModel.Name,
            Address = readModel.Address,
            City = readModel.City,
            PostalCode = readModel.PostalCode,
            Country = readModel.Country,
            Latitude = readModel.Latitude,
            Longitude = readModel.Longitude,
            Description = readModel.Description,
            CreatedAt = readModel.CreatedAt,
            TotalChargePoints = chargePoints.Count,
            AvailableConnectors = chargePoints.Sum(cp => 
                cp.Connectors?.Count(c => c.Status == "Available") ?? 0),
            ChargePoints = chargePoints.Select(cp => new ChargePointSummaryDto
            {
                Id = cp.Id,
                OcppChargerId = cp.OcppChargerId,
                LocationId = cp.LocationId,
                TotalConnectors = cp.Connectors?.Count ?? 0,
                MaxPower = cp.MaxPower,
                CurrentPower = cp.CurrentPower
            }).ToList()
        };
    }
}