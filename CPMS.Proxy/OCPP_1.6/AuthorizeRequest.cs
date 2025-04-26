namespace CPMS.Proxy.OCPP_1._6;

public class AuthorizeRequest
{
    [Newtonsoft.Json.JsonProperty("idTag", Required = Newtonsoft.Json.Required.Always)]
    [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
    [System.ComponentModel.DataAnnotations.StringLength(20)]
    public string IdTag { get; set; }
}