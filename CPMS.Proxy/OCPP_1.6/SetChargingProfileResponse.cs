namespace CPMS.Proxy.OCPP_1._6;

public class SetChargingProfileResponse
{
    [Newtonsoft.Json.JsonProperty("status", Required = Newtonsoft.Json.Required.Always)]
    [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
    [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
    public SetChargingProfileResponseStatus Status { get; set; }
}

public enum SetChargingProfileResponseStatus
{
    [System.Runtime.Serialization.EnumMember(Value = @"Accepted")]
    Accepted = 0,

    [System.Runtime.Serialization.EnumMember(Value = @"Rejected")]
    Rejected = 1,

    [System.Runtime.Serialization.EnumMember(Value = @"NotSupported")]
    NotSupported = 2,
}