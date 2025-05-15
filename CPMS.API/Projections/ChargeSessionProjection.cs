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
            model.Status = "Started";
            model.EnergyDeliveredKWh = 0;
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
    
            if (newValue.EnergyConsumed.HasValue)
            {
                model.EnergyDeliveredKWh = (newValue.EnergyConsumed.Value - model.StartMeterValue) / 1000.0;
            }
    
            return model;
        });
        
        ProjectEvent<ChargeSessionStoppedEvent>((model, @event) => {
            model.StopTime = @event.StopTime;
            model.StopMeterValue = @event.StopMeterValue;
            model.StopReason = @event.StopReason;
            model.Status = "Stopped";
            
            model.EnergyDeliveredKWh = (@event.StopMeterValue - model.StartMeterValue) / 1000.0;
            
            return model;
        });
    }
}