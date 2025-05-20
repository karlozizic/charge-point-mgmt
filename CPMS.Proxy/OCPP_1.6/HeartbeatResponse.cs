namespace CPMS.Proxy.OCPP_1._6;

public class HeartbeatResponse
{
    [Newtonsoft.Json.JsonProperty("currentTime", Required = Newtonsoft.Json.Required.Always)]
    [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
    public System.DateTimeOffset CurrentTime { get; set; }
}