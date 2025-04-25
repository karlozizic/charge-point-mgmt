using CPMS.API.BusinessRules;
using CPMS.API.DomainRules;
using CPMS.API.Events.ChargePoint;
using CPMS.BuildingBlocks.Domain;

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
            currentPower
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

    public void AddConnector(Guid connectorId, string name)
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

    public void UpdateConnectorStatus(Guid connectorId, string status)
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
}