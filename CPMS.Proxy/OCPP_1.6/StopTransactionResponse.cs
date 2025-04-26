namespace CPMS.Proxy.OCPP_1._6;

public class StopTransactionResponse
{
    [Newtonsoft.Json.JsonProperty("idTagInfo", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
    public IdTagInfo IdTagInfo { get; set; }
}