using CPMS.API.Events.ChargePoint;
using CPMS.API.Events.Connector;
using Marten.Events.Aggregation;
using ConnectorStatusChangedEvent = CPMS.API.Events.ChargePoint.ConnectorStatusChangedEvent;

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
            model.Connectors = new List<ConnectorReadModel>();
            return model;
        });
            
        ProjectEvent<ConnectorAddedEvent>((model, @event) => {
            model.Connectors.Add(new ConnectorReadModel {
                ConnectorId = @event.ConnectorId,
                Name = @event.ConnectorName,
                Status = "Available",
                LastStatusTime = DateTime.UtcNow
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
        
        ProjectEvent<ConnectorErrorLoggedEvent>((model, @event) =>
        {
            var connector = model.Connectors.FirstOrDefault(c =>
                c.ConnectorId == @event.ConnectorId);
            
            if (connector != null)
            {
                if (model.ConnectorErrors == null)
                    model.ConnectorErrors = new List<ConnectorErrorReadModel>();
                    
                model.ConnectorErrors.Add(new ConnectorErrorReadModel
                {
                    ConnectorId = @event.ConnectorId,
                    ErrorCode = @event.ErrorCode,
                    Info = @event.Info,
                    Timestamp = @event.Timestamp
                });
                
                //todo which status?
                connector.Status = "Faulted";
                connector.LastStatusTime = @event.Timestamp;
            }
            return model;
        });
    }
}