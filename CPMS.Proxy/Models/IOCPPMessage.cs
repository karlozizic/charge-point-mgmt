namespace CPMS.Proxy.Models;

public interface IOcppMessage
{
    string MessageType { get; set; }

    string UniqueId { get; set; }

    string Action { get; set; }

    string JsonPayload { get; set; }
}