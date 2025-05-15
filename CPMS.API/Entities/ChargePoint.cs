using CPMS.API.BusinessRules;
using CPMS.API.DomainRules;
using CPMS.API.Events.ChargePoint;
using CPMS.API.Events.Connector;
using CPMS.BuildingBlocks.Domain;
using ConnectorStatusChangedEvent = CPMS.API.Events.ChargePoint.ConnectorStatusChangedEvent;

namespace CPMS.API.Entities;

public class ChargePoint : Entity, IAggregateRoot
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public Guid LocationId { get; private set; }
    public double? MaxPower { get; private set; }
    public double? CurrentPower { get; private set; }

    private readonly List<Connector> _connectors = new List<Connector>();
    public IReadOnlyCollection<Connector> Connectors => _connectors.AsReadOnly();

    private ChargePoint()
    {
    }

    public ChargePoint(Guid id, string name, Guid locationId,
        double? maxPower, double? currentPower)
    {
        CheckRule(new ChargePointMustHaveValidNameRule(name));

        var @event = new ChargePointCreatedEvent(
            id,
            name,
            locationId,
            maxPower,
            0.0d
        );

        AddDomainEvent(@event);
        Apply(@event);
    }

    private void Apply(ChargePointCreatedEvent @event)
    {
        Id = @event.ChargePointId;
        Name = @event.Name;
        LocationId = @event.LocationId;
        MaxPower = @event.MaxPower;
        CurrentPower = @event.CurrentPower;
    }

    public void AddConnector(int connectorId, string name)
    {
        CheckRule(new ConnectorMustHaveUniqueIdRule(connectorId, _connectors));

        var @event = new ConnectorAddedEvent(Id, connectorId, name);

        AddDomainEvent(@event);
        Apply(@event);
    }

    private void Apply(ConnectorAddedEvent @event)
    {
        _connectors.Add(new Connector(@event.ConnectorId, @event.ConnectorName));
    }

    public void UpdateConnectorStatus(int connectorId, string status)
    {
        var connector = _connectors.SingleOrDefault(c => c.Id == connectorId);
        if (connector == null)
            throw new BusinessRuleValidationException(new ConnectorMustExistRule(connectorId));

        var @event = new ConnectorStatusChangedEvent(Id, connectorId, status, DateTime.UtcNow);

        AddDomainEvent(@event);
        Apply(@event);
    }

    private void Apply(ConnectorStatusChangedEvent @event)
    {
        var connector = _connectors.Single(c => c.Id == @event.ConnectorId);
        connector.UpdateStatus(@event.Status, @event.Timestamp);
    }
    
    public void RegisterBoot(
        string serial,
        string model,
        string vendor,
        string firmwareVersion)
    {
        var @event = new ChargePointBootedEvent(
            Id,
            serial,
            model,
            vendor,
            firmwareVersion,
            DateTime.UtcNow
        );
        
        AddDomainEvent(@event);
        Apply(@event);
    }

    private void Apply(ChargePointBootedEvent @event)
    {
        // TODO
        Console.WriteLine($"ChargePoint {Id} booted with serial { @event.Serial}");
    }
    
    public void LogConnectorError(int connectorId, string errorCode, string info)
    {
        var @event = new ConnectorErrorLoggedEvent(
            Id,
            connectorId,
            errorCode,
            info,
            DateTime.UtcNow
        );
        
        AddDomainEvent(@event);
        Apply(@event);
    }
    
    private void Apply(ConnectorErrorLoggedEvent @event)
    {
        var connector = _connectors.SingleOrDefault(c => c.Id == @event.ConnectorId);
        if (connector != null)
        {
            connector.LogError(@event.ErrorCode, @event.Info, @event.Timestamp);
        }
    }
    
    public void UpdateChargingProfile(int profileId, string profileData)
    {
        var @event = new ChargingProfileUpdatedEvent(
            Id,
            profileId,
            profileData
        );
        
        AddDomainEvent(@event);
        Apply(@event);
    }
    
    private void Apply(ChargingProfileUpdatedEvent @event)
    {
        //TODO Ažuriranje charging profile podataka
    }
    
    public void Reset(string resetType)
    {
        var @event = new ChargePointResetEvent(
            Id,
            resetType
        );
        
        AddDomainEvent(@event);
        Apply(@event);
    }
    
    private void Apply(ChargePointResetEvent @event)
    {
        //TODO Možemo resetirati statuse konektora ili druge informacije
    }
}