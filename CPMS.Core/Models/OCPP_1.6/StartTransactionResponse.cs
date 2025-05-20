namespace CPMS.Core.Models.OCPP_1._6;

public class StartTransactionResponse
{
    [Newtonsoft.Json.JsonProperty("idTagInfo", Required = Newtonsoft.Json.Required.Always)]
    [System.ComponentModel.DataAnnotations.Required]
    public IdTagInfo IdTagInfo { get; set; } = new IdTagInfo();

    [Newtonsoft.Json.JsonProperty("transactionId", Required = Newtonsoft.Json.Required.Always)]
    public int? TransactionId { get; set; }
}