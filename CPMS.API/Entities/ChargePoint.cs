using CPMS.API.BusinessRules;
using CPMS.API.Events.ChargePoint;
using CPMS.API.Events.Connector;
using CPMS.BuildingBlocks.Domain;

namespace CPMS.API.Entities;

public class ChargePoint : Entity, IAggregateRoot
{
    public Guid Id { get; private set; }
    public string OcppChargerId { get; private set; }
    public Guid LocationId { get; private set; }
    public double? MaxPower { get; private set; }
    public double? CurrentPower { get; private set; }

    private readonly List<Connector> _connectors = new();
    public IReadOnlyCollection<Connector> Connectors => _connectors.AsReadOnly();

    private ChargePoint()
    {
    }

    public ChargePoint(Guid id, string ocppChargerId, Guid locationId, double? maxPower, double? currentPower)
    {
        CheckRule(new ChargePointMustHaveValidNameRule(ocppChargerId));

        var @event = new ChargePointCreatedEvent(id, ocppChargerId, locationId, maxPower, currentPower ?? 0.0d);

        AddDomainEvent(@event);
        Apply(@event);
    }

    private void Apply(ChargePointCreatedEvent @event)
    {
        Id = @event.ChargePointId;
        OcppChargerId = @event.OcppChargerId;
        LocationId = @event.LocationId;
        MaxPower = @event.MaxPower;
        CurrentPower = @event.CurrentPower;
    }

    /// <summary>The next free connector id: OCPP connector ids are 1-based and dense.</summary>
    public int NextConnectorId() => _connectors.Count == 0 ? 1 : _connectors.Max(c => c.Id) + 1;

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
        if (_connectors.All(c => c.Id != connectorId))
            throw new BusinessRuleValidationException(new ConnectorMustExistRule(connectorId));

        var @event = new ConnectorStatusChangedEvent(Id, connectorId, status, DateTime.UtcNow);

        AddDomainEvent(@event);
        Apply(@event);
    }

    private void Apply(ConnectorStatusChangedEvent @event)
    {
        _connectors.Single(c => c.Id == @event.ConnectorId).UpdateStatus(@event.Status, @event.Timestamp);
    }

    public void RegisterBoot(string serial, string model, string vendor, string firmwareVersion)
    {
        var @event = new ChargePointBootedEvent(Id, serial, model, vendor, firmwareVersion, DateTime.UtcNow);

        AddDomainEvent(@event);
        Apply(@event);
    }

    private void Apply(ChargePointBootedEvent @event)
    {
        // Boot details are recorded in the stream only; no aggregate state depends on them yet.
    }

    public void LogConnectorError(int connectorId, string errorCode, string info)
    {
        var @event = new ConnectorErrorLoggedEvent(Id, connectorId, errorCode, info, DateTime.UtcNow);

        AddDomainEvent(@event);
        Apply(@event);
    }

    private void Apply(ConnectorErrorLoggedEvent @event)
    {
        _connectors.SingleOrDefault(c => c.Id == @event.ConnectorId)
            ?.LogError(@event.ErrorCode, @event.Info, @event.Timestamp);
    }
}
