namespace CPMS.API.Dtos;

public class ChargePointSummaryDto
{
    public Guid Id { get; set; }
    public string OcppChargerId { get; set; }
    public Guid LocationId { get; set; }
    public int TotalConnectors { get; set; }
    public double? MaxPower { get; set; }
    public double? CurrentPower { get; set; }
}