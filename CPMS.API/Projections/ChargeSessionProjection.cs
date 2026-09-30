using CPMS.API.Events.ChargeSession;
using Marten.Events.Aggregation;

namespace CPMS.API.Projections;

public class ChargeSessionProjection : SingleStreamProjection<ChargeSessionReadModel, Guid>
{
    public void Apply(ChargeSessionStartedEvent @event, ChargeSessionReadModel model)
    {
        model.Id = @event.ChargeSessionId;
        model.TransactionId = @event.TransactionId;
        model.ChargePointId = @event.ChargePointId.ToString();
        model.ConnectorId = @event.ConnectorId;
        model.TagId = @event.TagId;
        model.StartTime = @event.StartTime;
        model.StartMeterValue = @event.StartMeterValue;
        model.Status = nameof(SessionStatus.Started);
        model.EnergyDeliveredKWh = 0;
    }

    public void Apply(MeterValueRecordedEvent @event, ChargeSessionReadModel model)
    {
        if (@event.EnergyConsumed.HasValue && @event.EnergyConsumed.Value > 0)
        {
            model.EnergyDeliveredKWh = @event.EnergyConsumed.Value;
        }
    }

    public void Apply(ChargeSessionStoppedEvent @event, ChargeSessionReadModel model)
    {
        model.StopTime = @event.StopTime;
        model.DurationMinutes = (@event.StopTime - model.StartTime).TotalMinutes;
        model.StopMeterValue = @event.StopMeterValue;
        model.StopReason = @event.StopReason;
        model.Status = nameof(SessionStatus.Stopped);

        if (@event.StopMeterValue > 0 && model.StartMeterValue >= 0)
        {
            model.EnergyDeliveredKWh = (@event.StopMeterValue - model.StartMeterValue);
        }
    }
}
