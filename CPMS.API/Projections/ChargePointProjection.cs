using CPMS.API.Events.ChargePoint;
using Marten.Events.Aggregation;

namespace CPMS.API.Projections;

public class ChargePointProjection : SingleStreamProjection<ChargePointReadModel>
{
    public ChargePointProjection()
    {
        ProjectEvent<ChargePointCreatedEvent>((model, @event) => {
            model.Id = @event.ChargePointId;
            model.Name = @event.Name;
            model.LocationId = @event.LocationId;
            model.MaxPower = @event.MaxPower;
            return model;
        });
            
        ProjectEvent<ConnectorAddedEvent>((model, @event) => {
            model.Connectors.Add(new ConnectorReadModel {
                ConnectorId = @event.ConnectorId,
                Name = @event.ConnectorName
            });
            return model;
        });
            
        ProjectEvent<ConnectorStatusChangedEvent>((model, @event) =>
        {
            var connector = model.Connectors.FirstOrDefault(c =>
                c.ConnectorId == @event.ConnectorId);
            
            if (connector != null)
            {
                connector.Status = @event.Status;
                connector.LastStatusTime = @event.Timestamp;
            }
                
            return model;
        });
    }
}