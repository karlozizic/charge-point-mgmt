using CPMS.API.Events.ChargeSession;
using CPMS.API.Handlers.Billing;
using CPMS.API.Projections;
using Marten;
using MediatR;

namespace CPMS.API.Handlers.ChargeSession;

public class ChargeSessionStoppedEventHandler : INotificationHandler<ChargeSessionStoppedEvent>
{
    private readonly IMediator _mediator;
    private readonly IQuerySession _querySession;
    private readonly ILogger<ChargeSessionStoppedEventHandler> _logger;

    public ChargeSessionStoppedEventHandler(
        IMediator mediator,
        IQuerySession querySession,
        ILogger<ChargeSessionStoppedEventHandler> logger)
    {
        _mediator = mediator;
        _querySession = querySession;
        _logger = logger;
    }

    public async Task Handle(ChargeSessionStoppedEvent notification, CancellationToken cancellationToken)
    {
         try
        {
            _logger.LogInformation("Processing ChargeSessionStoppedEvent for session: {SessionId}", notification.ChargeSessionId);

            var sessionReadModel = await _querySession
                .Query<ChargeSessionReadModel>()
                .FirstOrDefaultAsync(s => s.Id == notification.ChargeSessionId, cancellationToken);

            if (sessionReadModel == null)
            {
                _logger.LogWarning("Session {SessionId} not found for billing", notification.ChargeSessionId);
                return;
            }

            _logger.LogInformation("Found session {SessionId}, ChargePoint: {ChargePointId}, Energy: {Energy} kWh", 
                sessionReadModel.Id, sessionReadModel.ChargePointId, sessionReadModel.EnergyDeliveredKWh);

            var existingBilling = await _querySession
                .Query<SessionBillingReadModel>()
                .FirstOrDefaultAsync(b => b.SessionId == notification.ChargeSessionId, cancellationToken);

            if (existingBilling != null)
            {
                _logger.LogInformation("Billing already exists for session {SessionId}", notification.ChargeSessionId);
                return;
            }

            var energyConsumed = sessionReadModel.EnergyDeliveredKWh ?? 0;
            
            if (energyConsumed <= 0)
            {
                energyConsumed = (notification.StopMeterValue - sessionReadModel.StartMeterValue) / 1000.0; // Wh to kWh
                _logger.LogInformation("Calculated energy from meter values: {Energy} kWh", energyConsumed);
            }

            await _mediator.Send(new CreateSessionBillingCommand
            {
                SessionId = notification.ChargeSessionId,
                EnergyConsumed = energyConsumed,
            }, cancellationToken);

            _logger.LogInformation("Successfully created billing for session {SessionId}", notification.ChargeSessionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create billing for session {SessionId}", notification.ChargeSessionId);
        }
    }
}