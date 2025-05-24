namespace CPMS.API.Dtos;

public class ChargeSessionDto
{
    public Guid Id { get; set; }
    public int TransactionId { get; set; }
    public string ChargePointId { get; set; }
    public string ChargePointName { get; set; }
    public int ConnectorId { get; set; }
    public string TagId { get; set; }
    public string TagName { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? StopTime { get; set; }
    public double StartMeterValue { get; set; }
    public double? StopMeterValue { get; set; }
    public double? EnergyDeliveredKWh { get; set; }
    public string Status { get; set; }
    public string? StopReason { get; set; }
    public TimeSpan? Duration => StopTime.HasValue ? StopTime.Value - StartTime : null;
    public List<MeterValueDto> MeterValues { get; set; } = new List<MeterValueDto>();
}