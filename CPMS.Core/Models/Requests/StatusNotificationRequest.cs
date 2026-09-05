namespace CPMS.Core.Models.Requests;

public class StatusNotificationRequest : BaseMessage
{
    public int OcppEvseId { get; set; }
    public int OcppConnectorId { get; set; }
    public string? LastStatus { get; set; }
    public DateTime? LastStatusTime { get; set; }
}
