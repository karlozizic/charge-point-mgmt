namespace CPMS.Core.Models.Responses;

public class StopTransactionResponse
{
    public int? SessionId { get; set; }
    public double? MeterStop { get; set; }
    public DateTime? TimeStop { get; set; }
    public string? StopTagId { get; set; }
    public string? StopReason { get; set; }
    public string TransactionId { get; set; }
}