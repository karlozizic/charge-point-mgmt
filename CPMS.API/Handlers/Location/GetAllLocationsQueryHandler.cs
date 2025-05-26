using CPMS.API.Dtos;
using CPMS.API.Projections;
using CPMS.Core.Common;
using Marten;
using MediatR;

namespace CPMS.API.Handlers.Location;

public class GetAllLocationsQuery : IRequest<PagedResult<LocationSummaryDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? City { get; set; }
    public string? Country { get; set; }
}

public class GetAllLocationsQueryHandler : IRequestHandler<GetAllLocationsQuery, PagedResult<LocationSummaryDto>>
{
    private readonly IQuerySession _querySession;

    public GetAllLocationsQueryHandler(IQuerySession querySession)
    {
        _querySession = querySession;
    }

    public async Task<PagedResult<LocationSummaryDto>> Handle(GetAllLocationsQuery request, CancellationToken cancellationToken)
    {
        IQueryable<LocationReadModel> query = _querySession.Query<LocationReadModel>();

        if (!string.IsNullOrEmpty(request.City))
        {
            query = query.Where(l => l.City == request.City);
        }

        if (!string.IsNullOrEmpty(request.Country))
        {
            query = query.Where(l => l.Country == request.Country);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var locations = await query
            .OrderBy(l => l.Name)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var locationIds = locations.Select(l => l.Id).ToList();
        
        var chargePointStats = await _querySession
            .Query<ChargePointReadModel>()
            .Where(cp => locationIds.Contains(cp.LocationId))
            .GroupBy(cp => cp.LocationId)
            .Select(g => new 
            { 
                LocationId = g.Key, 
                TotalChargePoints = g.Count(),
                AvailableConnectors = g.SelectMany(cp => cp.Connectors)
                                      .Count(c => c.Status == "Available")
            })
            .ToListAsync(cancellationToken);

        var locationDtos = locations.Select(location => 
        {
            var stats = chargePointStats.FirstOrDefault(s => s.LocationId == location.Id);
            return new LocationSummaryDto
            {
                Id = location.Id,
                Name = location.Name,
                Address = location.Address,
                City = location.City,
                Country = location.Country,
                TotalChargePoints = stats?.TotalChargePoints ?? 0,
                AvailableConnectors = stats?.AvailableConnectors ?? 0,
                CreatedAt = location.CreatedAt
            };
        }).ToList();

        return new PagedResult<LocationSummaryDto>(locationDtos, totalCount, request.PageNumber, request.PageSize);
    }
}