using CPMS.API.Projections;
using CPMS.API.Repositories;
using CPMS.Core.Models.OCPP_1._6;
using CPMS.Core.Models.Responses;
using Marten;
using MediatR;

namespace CPMS.API.Handlers.ChargeSession;

public class StopTransactionCommand : IRequest<StopTransactionResponse>
{
    public int TranscationId { get; set; }
    public double? MeterStop { get; set; }
    public DateTime? TimeStop { get; set; }
    public string? StopTagId { get; set; }
    public string? StopReason { get; set; }

    public StopTransactionCommand(
        int transactionId,
        DateTime? timeStop,
        double? meterStop,
        string? tagId,
        string? reason)
    {
        TranscationId = transactionId;
        TimeStop = timeStop;
        MeterStop = meterStop;
        StopTagId = tagId;
        StopReason = reason;
    }
}

public class StopTransactionCommandHandler : IRequestHandler<StopTransactionCommand, StopTransactionResponse>
{
    private readonly IAggregateRepository<Entities.ChargeSession> _chargeSessions;
    private readonly IAggregateRepository<Entities.ChargePoint> _chargePoints;
    private readonly IQuerySession _querySession;
    private readonly ILogger<StopTransactionCommandHandler> _logger;

    public StopTransactionCommandHandler(
        IAggregateRepository<Entities.ChargeSession> chargeSessions,
        IAggregateRepository<Entities.ChargePoint> chargePoints,
        IQuerySession querySession,
        ILogger<StopTransactionCommandHandler> logger)
    {
        _chargeSessions = chargeSessions;
        _chargePoints = chargePoints;
        _querySession = querySession;
        _logger = logger;
    }

    public async Task<StopTransactionResponse> Handle(StopTransactionCommand command, CancellationToken cancellationToken)
    {
        var readModel = await _querySession.Query<ChargeSessionReadModel>()
            .FirstOrDefaultAsync(cs => cs.TransactionId == command.TranscationId, cancellationToken);
        var chargeSession = readModel == null ? null : await _chargeSessions.LoadAsync(readModel.Id, cancellationToken);

        if (chargeSession == null)
        {
            _logger.LogWarning("Received stop transaction for unknown transaction {TransactionId}", command.TranscationId);
            return Invalid();
        }

        if (command.StopTagId == null || command.MeterStop == null)
            throw new ArgumentNullException(nameof(command), "StopTagId and MeterStop cannot be null");

        chargeSession.StopCharging(command.StopTagId, command.MeterStop.Value, command.StopReason);
        await _chargeSessions.SaveAsync(chargeSession, cancellationToken);

        var chargePoint = await _chargePoints.LoadAsync(chargeSession.ChargePointId, cancellationToken);
        if (chargePoint == null)
            return Invalid();

        if (chargePoint.Connectors.Any(c => c.Id == chargeSession.ConnectorId))
        {
            chargePoint.UpdateConnectorStatus(chargeSession.ConnectorId, "Available");
            await _chargePoints.SaveAsync(chargePoint, cancellationToken);
        }

        return new StopTransactionResponse
        {
            IdTagInfo = new IdTagInfo { Status = AuthorizationStatus.Accepted }
        };
    }

    private static StopTransactionResponse Invalid() => new()
    {
        IdTagInfo = new IdTagInfo { Status = AuthorizationStatus.Invalid }
    };
}
