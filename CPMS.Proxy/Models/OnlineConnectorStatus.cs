namespace CPMS.Proxy.Models;

public class OnlineConnectorStatus
{
    public ConnectorStatusEnum Status { get; set; }

    public double? ChargeRateKw { get; set; } 
    
    public double? MeterKwh { get; set; }

    public double? SoC { get; set; }
}