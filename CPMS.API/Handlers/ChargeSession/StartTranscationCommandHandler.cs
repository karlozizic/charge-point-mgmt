using CPMS.API.BusinessRules;
using CPMS.API.Exceptions;
using CPMS.API.Projections;
using CPMS.API.Repositories;
using CPMS.BuildingBlocks.Domain;
using CPMS.Core.Models.OCPP_1._6;
using CPMS.Core.Models.Responses;
using Marten;
using MediatR;

namespace CPMS.API.Handlers.ChargeSession;

public class StartTransactionCommand : IRequest<StartTransactionResponse>
{
    public string OcppChargerId { get; set; }
    public int ConnectorId { get; set; }
    public string TagId { get; set; }
    public double MeterStart { get; set; }
}

public class StartTransactionCommandHandler : IRequestHandler<StartTransactionCommand, StartTransactionResponse>
{
    private readonly IAggregateRepository<Entities.ChargePoint> _chargePoints;
    private readonly IAggregateRepository<Entities.ChargeSession> _chargeSessions;
    private readonly IQuerySession _querySession;

    public StartTransactionCommandHandler(
        IAggregateRepository<Entities.ChargePoint> chargePoints,
        IAggregateRepository<Entities.ChargeSession> chargeSessions,
        IQuerySession querySession)
    {
        _chargePoints = chargePoints;
        _chargeSessions = chargeSessions;
        _querySession = querySession;
    }

    public async Task<StartTransactionResponse> Handle(StartTransactionCommand command, CancellationToken cancellationToken)
    {
        var tag = await _querySession
            .Query<ChargeTagReadModel>()
            .FirstOrDefaultAsync(t => t.TagId == command.TagId, cancellationToken);

        if (tag == null || tag.Blocked || (tag.ExpiryDate.HasValue && tag.ExpiryDate.Value <= DateTime.UtcNow))
            throw new BusinessRuleValidationException(new TagNotValidRule(command.TagId));

        var chargePointReadModel = await _querySession.Query<ChargePointReadModel>()
            .FirstOrDefaultAsync(cp => cp.OcppChargerId == command.OcppChargerId, cancellationToken);
        var chargePoint = chargePointReadModel == null
            ? null
            : await _chargePoints.LoadAsync(chargePointReadModel.Id, cancellationToken);

        if (chargePoint == null)
            throw new NotFoundException($"Charge point {command.OcppChargerId} not found");

        if (chargePoint.Connectors.All(c => c.Id != command.ConnectorId))
            throw new NotFoundException($"Connector {command.ConnectorId} not found on charge point {command.OcppChargerId}");

        // Known gap: no uniqueness check on the OCPP transaction id.
        var transactionId = Random.Shared.Next(1, 1_000_000);

        var chargeSession = new Entities.ChargeSession(
            Guid.NewGuid(),
            transactionId,
            chargePoint.Id,
            command.ConnectorId,
            command.TagId,
            command.MeterStart);

        await _chargeSessions.SaveAsync(chargeSession, cancellationToken);

        chargePoint.UpdateConnectorStatus(command.ConnectorId, "Charging");
        await _chargePoints.SaveAsync(chargePoint, cancellationToken);

        return new StartTransactionResponse
        {
            TransactionId = transactionId,
            IdTagInfo = new IdTagInfo
            {
                Status = AuthorizationStatus.Accepted,
                ExpiryDate = tag.ExpiryDate.HasValue ? new DateTimeOffset(tag.ExpiryDate.Value) : null
            }
        };
    }
}
