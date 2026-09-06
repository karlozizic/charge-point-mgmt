using CPMS.API.Events.ChargePoint;
using CPMS.API.Events.Connector;
using Marten.Events.Aggregation;

namespace CPMS.API.Projections;

public class ChargePointProjection : SingleStreamProjection<ChargePointReadModel, Guid>
{
    public ChargePointProjection()
    {
        ProjectEvent<ChargePointCreatedEvent>((model, @event) => {
            model.Id = @event.ChargePointId;
            model.OcppChargerId = @event.OcppChargerId;
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
                // Events written before AddedAt existed deserialize to default. Leave those null
                // rather than showing year 1 as if it were a real reading.
                LastStatusTime = @event.AddedAt == default ? null : @event.AddedAt
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
                    Id = @event.ErrorId,
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