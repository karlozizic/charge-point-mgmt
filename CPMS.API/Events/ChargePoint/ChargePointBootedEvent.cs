using CPMS.BuildingBlocks.Domain;

namespace CPMS.API.Events.ChargePoint;

public class ChargePointBootedEvent : DomainEventBase
{
    public Guid ChargePointId { get; }
    public string Serial { get; }
    public string Model { get; }
    public string Vendor { get; }
    public string FirmwareVersion { get; }
    public DateTime BootTime { get; }
        
    public ChargePointBootedEvent(
        Guid chargePointId,
        string serial,
        string model,
        string vendor,
        string firmwareVersion,
        DateTime bootTime)
    {
        ChargePointId = chargePointId;
        Serial = serial;
        Model = model;
        Vendor = vendor;
        FirmwareVersion = firmwareVersion;
        BootTime = bootTime;
    }
}