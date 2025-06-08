using CPMS.BuildingBlocks.Domain;

namespace CPMS.API.Events.PricingGroup;

public class ChargePointAssignedToPricingGroupEvent : DomainEventBase
{
    public Guid PricingGroupId { get; }
    public Guid ChargePointId { get; }
    public DateTime AssignedAt { get; }
    
    public ChargePointAssignedToPricingGroupEvent(
        Guid pricingGroupId,
        Guid chargePointId,
        DateTime assignedAt)
    {
        PricingGroupId = pricingGroupId;
        ChargePointId = chargePointId;
        AssignedAt = assignedAt;
    }
}