namespace CPMS.Proxy.Models;

/// <summary>One OCPP-J frame: CALL (2), CALLRESULT (3) or CALLERROR (4).</summary>
public class OCPPMessage
{
    public string MessageType { get; set; }
    public string UniqueId { get; set; }
    public string Action { get; set; }
    public string JsonPayload { get; set; }
    public string? ErrorCode { get; set; }
    public string ErrorDescription { get; set; }
}
