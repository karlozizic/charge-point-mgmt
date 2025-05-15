using CPMS.API.BusinessRules;
using CPMS.API.Exceptions;
using CPMS.API.Projections;
using CPMS.API.Repositories;
using CPMS.BuildingBlocks.Domain;
using Marten;
using MediatR;

namespace CPMS.API.Handlers.ChargeSession;

public class StartTransactionCommand : IRequest<int>
{
    public string OcppChargerId { get; set; }
    public int ConnectorId { get; set; }
    public string TagId { get; set; }
    public double MeterStart { get; set; }
}

public class StartTransactionCommandHandler : IRequestHandler<StartTransactionCommand, int>
{
    private readonly IChargePointRepository _chargePointRepository;
    private readonly IChargeSessionRepository _chargeSessionRepository;
    private readonly IQuerySession _querySession;
    private static readonly Random _random = new Random();
    
    public StartTransactionCommandHandler(
        IChargePointRepository chargePointRepository,
        IChargeSessionRepository chargeSessionRepository,
        IQuerySession querySession)
    {
        _chargePointRepository = chargePointRepository;
        _chargeSessionRepository = chargeSessionRepository;
        _querySession = querySession;
    }
    
    public async Task<int> Handle(StartTransactionCommand command, CancellationToken cancellationToken)
    {
        var tag = await _querySession
            .Query<ChargeTagReadModel>()
            .FirstOrDefaultAsync(t => t.TagId == command.TagId, cancellationToken);
            
        if (tag == null || tag.Blocked || (tag.ExpiryDate.HasValue && tag.ExpiryDate.Value <= DateTime.UtcNow))
            throw new BusinessRuleValidationException(new TagNotValidRule(command.TagId));
        
        var chargePoint = await _chargePointRepository.GetByOcppChargerIdAsync(command.OcppChargerId);
        
        if (chargePoint == null)
            throw new NotFoundException($"Charge point {command.OcppChargerId} not found");
        
        var connector = chargePoint.Connectors.FirstOrDefault(c => c.Id == command.ConnectorId);
            
        if (connector == null)
            throw new NotFoundException($"Connector {command.ConnectorId} not found on charge point {command.OcppChargerId}");
        
        var sessionId = Guid.NewGuid();
        var transactionId = _random.Next(1, 1000000);
        
        var chargeSession = new Entities.ChargeSession(
            sessionId,
            transactionId,
            chargePoint.Id,
            command.ConnectorId,
            command.TagId,
            command.MeterStart);
            
        await _chargeSessionRepository.AddAsync(chargeSession);
        
        chargePoint.UpdateConnectorStatus(connector.Id, "Charging");
        await _chargePointRepository.UpdateAsync(chargePoint);
        
        return transactionId;
    }
}