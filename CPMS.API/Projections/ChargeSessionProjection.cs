using CPMS.API.Events.ChargeSession;
using Marten.Events.Aggregation;

namespace CPMS.API.Projections;

public class ChargeSessionProjection : SingleStreamProjection<ChargeSessionReadModel>
{
    public ChargeSessionProjection()
    {
        ProjectEvent<ChargeSessionStartedEvent>((model, @event) =>
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
            model.MeterValues = new List<MeterValueReadModel>();
            return model;
        });
        
        ProjectEvent<MeterValueRecordedEvent>((model, @event) => {
            var newValue = new MeterValueReadModel { 
                SessionId = @event.ChargeSessionId,
                TransactionId = @event.TransactionId,
                Timestamp = @event.Timestamp,
                CurrentPower = @event.CurrentPower,
                EnergyConsumed = @event.EnergyConsumed,
                StateOfCharge = @event.StateOfCharge
            };
    
            model.MeterValues.Add(newValue);
            
            if (@event.EnergyConsumed.HasValue && @event.EnergyConsumed.Value > 0)
            {
                model.EnergyDeliveredKWh = @event.EnergyConsumed.Value;
            }
    
            return model;
        });
        
        ProjectEvent<ChargeSessionStoppedEvent>((model, @event) => {
            model.StopTime = @event.StopTime;
            model.StopMeterValue = @event.StopMeterValue;
            model.StopReason = @event.StopReason;
            model.Status = nameof(SessionStatus.Stopped);
            
            if (@event.StopMeterValue > 0 && model.StartMeterValue >= 0)
            {
                model.EnergyDeliveredKWh = (@event.StopMeterValue - model.StartMeterValue);
            }
            else
            {
                var lastMeterValue = model.MeterValues
                    .Where(mv => mv.EnergyConsumed.HasValue)
                    .OrderByDescending(mv => mv.Timestamp)
                    .FirstOrDefault();
                    
                if (lastMeterValue?.EnergyConsumed > 0)
                {
                    model.EnergyDeliveredKWh = lastMeterValue.EnergyConsumed.Value;
                }
            }
            
            return model;
        });
    }
}