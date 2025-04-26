namespace CPMS.Proxy.OCPP_1._6;

public class ClearChargingProfileRequest
{
    [Newtonsoft.Json.JsonProperty("id", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
    public int Id { get; set; }

    [Newtonsoft.Json.JsonProperty("connectorId", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
    public int ConnectorId { get; set; }

    [Newtonsoft.Json.JsonProperty("chargingProfilePurpose", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
    [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
    public ClearChargingProfileRequestChargingProfilePurpose ChargingProfilePurpose { get; set; }

    [Newtonsoft.Json.JsonProperty("stackLevel", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
    public int StackLevel { get; set; }
}

public enum ClearChargingProfileRequestChargingProfilePurpose
{
    [System.Runtime.Serialization.EnumMember(Value = @"ChargePointMaxProfile")]
    ChargePointMaxProfile = 0,

    [System.Runtime.Serialization.EnumMember(Value = @"TxDefaultProfile")]
    TxDefaultProfile = 1,

    [System.Runtime.Serialization.EnumMember(Value = @"TxProfile")]
    TxProfile = 2,
}