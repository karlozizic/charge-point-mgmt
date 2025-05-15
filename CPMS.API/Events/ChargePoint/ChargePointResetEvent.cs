using CPMS.BuildingBlocks.Domain;

namespace CPMS.API.Events.ChargePoint;

public class ChargePointResetEvent : DomainEventBase
{
    public Guid ChargePointId { get; }
    public string ResetType { get; }
    
    public ChargePointResetEvent(Guid chargePointId, string resetType)
    {
        ChargePointId = chargePointId;
        ResetType = resetType;
    }
}