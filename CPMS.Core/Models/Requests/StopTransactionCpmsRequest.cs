namespace CPMS.Core.Models.Requests;

public class StopTransactionCpmsRequest
{
    public int? TranscationId { get; set; }
    public double? MeterStop { get; set; }
    public DateTime? TimeStop { get; set; }
    public string? StopTagId { get; set; }
    public string? StopReason { get; set; }
}