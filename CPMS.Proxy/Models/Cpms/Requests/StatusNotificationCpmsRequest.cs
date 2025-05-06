namespace CPMS.Proxy.Models.Cpms.Requests;

public class StatusNotificationCpmsRequest : BaseMessage
{
    public string OcppChargerId { get; set; }
    public int OcppEvseId { get; set; }
    public int OcppConnectorId { get; set; }
    public string? LastStatus { get; set; }
    public DateTime? LastStatusTime { get; set; }
}