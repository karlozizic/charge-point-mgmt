namespace CPMS.Proxy.Models.Cpms.Responses;

public class ResetChargerResponse : BaseMessage
{
    public string Status { get; set; }
    public string Reason { get; set; }
}