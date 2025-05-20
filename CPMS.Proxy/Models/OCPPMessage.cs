using Newtonsoft.Json;

namespace CPMS.Proxy.Models;

public class OCPPMessage : IOcppMessage
{
    public string MessageType { get; set; }

    public string UniqueId { get; set; }

    public string Action { get; set; }

    public string JsonPayload { get; set; }

    public string? ErrorCode { get; set; }

    public string ErrorDescription { get; set; }

    [JsonIgnore]
    public TaskCompletionSource<string> TaskCompletionSource { get; set; }

    public OCPPMessage()
    {
    }

    public OCPPMessage(string messageType, string uniqueId, string action, string jsonPayload)
    {
        MessageType = messageType;
        UniqueId = uniqueId;
        Action = action;
        JsonPayload = jsonPayload;
    }
}