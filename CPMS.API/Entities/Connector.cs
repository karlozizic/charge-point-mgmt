using CPMS.BuildingBlocks.Domain;

namespace CPMS.API.Entities;

public class Connector : Entity
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Status { get; private set; }
    public DateTime? LastStatusTime { get; private set; }
    public double? LastMeter { get; private set; }
    public DateTime? LastMeterTime { get; private set; }

    private Connector()
    {
    }

    public Connector(Guid id, string name)
    {
        Id = id;
        Name = name;
    }
        
    public void UpdateStatus(string status, DateTime timestamp)
    {
        Status = status;
        LastStatusTime = timestamp;
    }
        
    public void UpdateMeter(double meterValue, DateTime timestamp)
    {
        LastMeter = meterValue;
        LastMeterTime = timestamp;
    }
}