using CPMS.Core.Models.Enums;
using CPMS.Core.Models.Responses;

namespace CPMS.Core.Models.OCPP_1._6;

public class IdTagInfo
{
    [Newtonsoft.Json.JsonProperty("expiryDate", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
    public DateTimeOffset ExpiryDate { get; set; }

    [Newtonsoft.Json.JsonProperty("parentIdTag", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
    [System.ComponentModel.DataAnnotations.StringLength(20)]
    public string ParentIdTag { get; set; }

    [Newtonsoft.Json.JsonProperty("status", Required = Newtonsoft.Json.Required.Always)]
    [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
    [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
    public AuthorizationStatus Status { get; set; }
}