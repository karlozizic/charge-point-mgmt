using System.Runtime.Serialization;

namespace CPMS.Proxy.OCPP_1._6;

public class ResetRequest
{
    [Newtonsoft.Json.JsonProperty("type", Required = Newtonsoft.Json.Required.Always)]
    [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
    [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
    [DataMember (Name = "type")]
    public ResetType Type { get; set; }
}

public enum ResetType
{
    [System.Runtime.Serialization.EnumMember(Value = @"Hard")]
    Hard = 0,

    [System.Runtime.Serialization.EnumMember(Value = @"Soft")]
    Soft = 1,
}
