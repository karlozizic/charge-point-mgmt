using CPMS.API.BusinessRules;
using CPMS.API.Events.ChargeSession;
using CPMS.API.Handlers.ChargeSession;
using CPMS.BuildingBlocks.Domain;
using CPMS.Core.Models.Requests;

namespace CPMS.API.Entities;

public class ChargeSession : Entity, IAggregateRoot
{
    public Guid Id { get; private set; }
    public int TransactionId { get; private set; }
    public Guid ChargePointId { get; private set; }
    public int ConnectorId { get; private set; }
    public string TagId { get; private set; }
    public DateTime StartTime { get; private set; }
    public DateTime? StopTime { get; private set; }
    public double StartMeterValue { get; private set; }
    public double? StopMeterValue { get; private set; }
    public string Status { get; private set; }
    
    private readonly List<MeterValue> _meterValues = new List<MeterValue>();
    public IReadOnlyCollection<MeterValue> MeterValues => _meterValues.AsReadOnly();
    
    private ChargeSession() { }

    public ChargeSession(
        Guid id,
        int transactionId,
        Guid chargePointId,
        int connectorId,
        string tagId,
        double startMeterValue)
    {
        var @event = new ChargeSessionStartedEvent(
            id,
            transactionId,
            chargePointId,
            connectorId,
            tagId,
            DateTime.UtcNow,
            startMeterValue
            );

        AddDomainEvent(@event);
        Apply(@event);
    }
    
    private void Apply(ChargeSessionStartedEvent @event)
    {
        Id = @event.ChargeSessionId;
        TransactionId = @event.TransactionId;
        ChargePointId = @event.ChargePointId;
        ConnectorId = @event.ConnectorId;
        TagId = @event.TagId;
        StartTime = @event.StartTime;
        StartMeterValue = @event.StartMeterValue;
        Status = "Started";
    }

    public void AddMeterValue(MeterValuesCommand request, DateTime timestamp)
    {
        if (StopTime.HasValue)
            throw new BusinessRuleValidationException(new CannotAddMeterValuesAfterStopRule());

        var @event = new MeterValueRecordedEvent(
            Id,
            request.TransactionId,
            ChargePointId,
            ConnectorId,
            timestamp,
            request.CurrentPower,
            request.EnergyConsumed,
            request.StateOfCharge
            );
        
        AddDomainEvent(@event);
        Apply(@event);
    }
    
    private void Apply(MeterValueRecordedEvent @event)
    {
        _meterValues.Add(new MeterValue(@event.Timestamp, @event.ChargeSessionId, @event.CurrentPower, @event.EnergyConsumed, @event.StateOfCharge));
    }
        
    public void StopCharging(string tagId, double stopMeterValue, string stopReason)
    {
        if (StopTime.HasValue)
            throw new BusinessRuleValidationException(new CannotStopAlreadyStoppedSessionRule());
            
        var @event = new ChargeSessionStoppedEvent(
            Id,
            ChargePointId,
            ConnectorId,
            tagId,
            DateTime.UtcNow,
            stopMeterValue,
            stopReason
        );
            
        AddDomainEvent(@event);
        Apply(@event);
    }
        
    private void Apply(ChargeSessionStoppedEvent @event)
    {
        StopTime = @event.StopTime;
        StopMeterValue = @event.StopMeterValue;
        Status = "Stopped";
    }
}