using Newtonsoft.Json;
using System.Net.WebSockets;
using CPMS.API.Entities;

namespace CPMS.Proxy.Models;

public class ChargePointStatus
{
    private Dictionary<int, OnlineConnectorStatus> _onlineConnectors;

    public ChargePointStatus()
    {
    }

    public ChargePointStatus(ChargePoint chargePoint)
    {
        Id = chargePoint.Id.ToString();
        Name = chargePoint.Name;
    }

    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

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