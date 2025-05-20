using CPMS.Core.Models.OCPP_1._6;

namespace CPMS.Proxy.OCPP_1._6;

public class AuthorizeResponse
{
    [Newtonsoft.Json.JsonProperty("idTagInfo", Required = Newtonsoft.Json.Required.Always)]
    [System.ComponentModel.DataAnnotations.Required]
    public IdTagInfo IdTagInfo { get; set; } = new IdTagInfo();
}

public enum IdTagInfoStatus
{
    [System.Runtime.Serialization.EnumMember(Value = @"Accepted")]
    Accepted = 0,

    [System.Runtime.Serialization.EnumMember(Value = @"Blocked")]
    Blocked = 1,

    [System.Runtime.Serialization.EnumMember(Value = @"Expired")]
    Expired = 2,

    [System.Runtime.Serialization.EnumMember(Value = @"Invalid")]
    Invalid = 3,

    [System.Runtime.Serialization.EnumMember(Value = @"ConcurrentTx")]
    ConcurrentTx = 4,
}