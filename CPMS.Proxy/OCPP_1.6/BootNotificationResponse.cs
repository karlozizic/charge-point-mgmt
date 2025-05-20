namespace CPMS.Proxy.OCPP_1._6;

public class BootNotificationResponse
{
    [Newtonsoft.Json.JsonProperty("status", Required = Newtonsoft.Json.Required.Always)]
    [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
    [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
    public BootNotificationResponseStatus Status { get; set; }

    [Newtonsoft.Json.JsonProperty("currentTime", Required = Newtonsoft.Json.Required.Always)]
    [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
    public System.DateTimeOffset CurrentTime { get; set; }

    [Newtonsoft.Json.JsonProperty("interval", Required = Newtonsoft.Json.Required.Always)]
    public int Interval { get; set; }
}

public enum BootNotificationResponseStatus
{
    [System.Runtime.Serialization.EnumMember(Value = @"Accepted")]
    Accepted = 0,

    [System.Runtime.Serialization.EnumMember(Value = @"Pending")]
    Pending = 1,

    [System.Runtime.Serialization.EnumMember(Value = @"Rejected")]
    Rejected = 2,
}