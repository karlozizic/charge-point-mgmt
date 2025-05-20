namespace CPMS.Proxy.OCPP_1._6;

public class UnlockConnectorResponse
{
    [Newtonsoft.Json.JsonProperty("status", Required = Newtonsoft.Json.Required.Always)]
    [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
    [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
    public UnlockConnectorResponseStatus Status { get; set; }
}

public enum UnlockConnectorResponseStatus
{
    [System.Runtime.Serialization.EnumMember(Value = @"Unlocked")]
    Unlocked = 0,

    [System.Runtime.Serialization.EnumMember(Value = @"UnlockFailed")]
    UnlockFailed = 1,

    [System.Runtime.Serialization.EnumMember(Value = @"NotSupported")]
    NotSupported = 2
}