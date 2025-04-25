using CPMS.BuildingBlocks.Domain;

namespace CPMS.API.Events.ChargePoint;

public class ChargePointCreatedEvent : DomainEventBase
{
    public Guid ChargePointId { get; }
    public string Name { get; }
    public Guid LocationId { get; }
    public double? MaxPower { get; }
    public double? CurrentPower { get; }
        
    public ChargePointCreatedEvent(Guid chargePointId, string name, Guid locationId,
        double? maxPower, double? currentPower)
    {
        ChargePointId = chargePointId;
        Name = name;
        LocationId = locationId;
        MaxPower = maxPower;
        CurrentPower = currentPower;
    }
}