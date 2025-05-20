namespace CPMS.Proxy.OCPP_1._6;

public class ClearChargingProfileResponse
{
    [Newtonsoft.Json.JsonProperty("status", Required = Newtonsoft.Json.Required.Always)]
    [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
    [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
    public ClearChargingProfileResponseStatus Status { get; set; }
}

public enum ClearChargingProfileResponseStatus
{
    [System.Runtime.Serialization.EnumMember(Value = @"Accepted")]
    Accepted = 0,

    [System.Runtime.Serialization.EnumMember(Value = @"Unknown")]
    Unknown = 1,
}