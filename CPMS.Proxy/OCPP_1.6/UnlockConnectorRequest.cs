namespace CPMS.Proxy.OCPP_1._6;

public class UnlockConnectorRequest
{
    [Newtonsoft.Json.JsonProperty("connectorId", Required = Newtonsoft.Json.Required.Always)]
    public int ConnectorId { get; set; }
}