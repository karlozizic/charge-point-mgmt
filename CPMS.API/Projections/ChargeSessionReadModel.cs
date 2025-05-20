namespace CPMS.API.Projections;

public class ChargeSessionReadModel
{
    public Guid Id { get; set; }
    public int TransactionId { get; set; }
    public string ChargePointId { get; set; }
    public int ConnectorId { get; set; }
    public string TagId { get; set; }
    public string TagName { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? StopTime { get; set; }
    public double StartMeterValue { get; set; }
    public double? StopMeterValue { get; set; }
    public double? EnergyDeliveredKWh { get; set; }
    public string Status { get; set; }
    public string StopReason { get; set; }
    public List<MeterValueReadModel> MeterValues { get; set; } = new List<MeterValueReadModel>();
}