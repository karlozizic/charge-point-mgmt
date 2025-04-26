namespace CPMS.Proxy.OCPP_1._6;

public class DataTransferResponse
{
    [Newtonsoft.Json.JsonProperty("status", Required = Newtonsoft.Json.Required.Always)]
    [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
    [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
    public DataTransferResponseStatus Status { get; set; }

    [Newtonsoft.Json.JsonProperty("data", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
    public string Data { get; set; }

}

public enum DataTransferResponseStatus
{
    [System.Runtime.Serialization.EnumMember(Value = @"Accepted")]
    Accepted = 0,

    [System.Runtime.Serialization.EnumMember(Value = @"Rejected")]
    Rejected = 1,

    [System.Runtime.Serialization.EnumMember(Value = @"UnknownMessageId")]
    UnknownMessageId = 2,

    [System.Runtime.Serialization.EnumMember(Value = @"UnknownVendorId")]
    UnknownVendorId = 3,

}