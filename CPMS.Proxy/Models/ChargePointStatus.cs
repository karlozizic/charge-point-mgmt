using System.Net.WebSockets;

namespace CPMS.Proxy.Models;

/// <summary>One connected charger: its OCPP id, negotiated protocol and open socket.</summary>
public class ChargePointStatus
{
    public required string Id { get; init; }
    public required string Protocol { get; init; }
    public required WebSocket WebSocket { get; init; }
}
