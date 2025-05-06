namespace CPMS.Core.Models.Responses;

public class StartTransactionChargerResponse
{
    public string OcppChargerId { get; set; }
    public int OcppConnectorId { get; set; }
    public int OcppEvseId { get; set; }
    public string? IdTag { get; set; }
    public double? MeterStart { get; set; }
    public DateTimeOffset? TimeStart { get; set; }
    public string TransactionId { get; set; }
    public int? RemoteStartId { get; set; }
}