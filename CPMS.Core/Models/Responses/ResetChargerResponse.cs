namespace CPMS.Core.Models.Responses;

public class ResetChargerResponse : BaseMessage
{
    public string Status { get; set; }
    public string Reason { get; set; }
}