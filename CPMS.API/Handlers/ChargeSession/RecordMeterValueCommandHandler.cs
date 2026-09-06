using CPMS.API.Projections;
using CPMS.API.Repositories;
using CPMS.Core.Models.Requests;
using Marten;
using MediatR;

namespace CPMS.API.Handlers.ChargeSession;

public record MeterValuesCommand(MeterValuesRequest Request) : IRequest;

public class MeterValuesCommandHandler : IRequestHandler<MeterValuesCommand>
{
    private readonly IAggregateRepository<Entities.ChargeSession> _chargeSessions;
    private readonly IQuerySession _querySession;
    private readonly ILogger<MeterValuesCommandHandler> _logger;

    public MeterValuesCommandHandler(
        IAggregateRepository<Entities.ChargeSession> chargeSessions,
        IQuerySession querySession,
        ILogger<MeterValuesCommandHandler> logger)
    {
        _chargeSessions = chargeSessions;
        _querySession = querySession;
        _logger = logger;
    }

    public async Task Handle(MeterValuesCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;

        var readModel = await _querySession.Query<ChargeSessionReadModel>()
            .FirstOrDefaultAsync(cs => cs.TransactionId == request.TransactionId, cancellationToken);
        var chargeSession = readModel == null ? null : await _chargeSessions.LoadAsync(readModel.Id, cancellationToken);

        if (chargeSession == null)
        {
            _logger.LogWarning("Received meter value for unknown transaction {TransactionId}", request.TransactionId);
            return;
        }

        // Server time, as before. The charger's own MeterTime is not trusted for ordering yet.
        chargeSession.AddMeterValue(
            request.CurrentPower,
            request.EnergyConsumed,
            request.StateOfCharge,
            DateTime.UtcNow);

        await _chargeSessions.SaveAsync(chargeSession, cancellationToken);
    }
}
