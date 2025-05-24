namespace CPMS.API.Dtos;

public class MeterValueDto
{
    public DateTime Timestamp { get; set; }
    public double? CurrentPower { get; set; }
    public double? EnergyConsumed { get; set; }
    public double? StateOfCharge { get; set; }
    public double? Voltage { get; set; }
    public double? Current { get; set; }
    public double? Temperature { get; set; }
}