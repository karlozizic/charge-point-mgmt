using Newtonsoft.Json;
using System.Net.WebSockets;

namespace CPMS.Proxy.Models;

public class ChargePointStatus
{
    private Dictionary<int, OnlineConnectorStatus> _onlineConnectors;

    public ChargePointStatus()
    {
    }

    public ChargePointStatus(Guid chargePointId)
    {
        Id = chargePointId.ToString();
    }

    //TODO: Charge Point status is casted to Guid in HandleAuthorize
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("protocol")]
    public string Protocol { get; set; }

    public Dictionary<int, OnlineConnectorStatus> OnlineConnectors
    {
        get
        {
            return _onlineConnectors;
        }
        set
        {
            _onlineConnectors = value;
        }
    }

    [JsonIgnore]
    public WebSocket WebSocket { get; set; }
}