using MediatR;

namespace CPMS.Core.Models.Requests;

public class BootNotificationRequest : BaseMessage
{
    [Newtonsoft.Json.JsonProperty("chargePointVendor", Required = Newtonsoft.Json.Required.Always)]
    [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
    [System.ComponentModel.DataAnnotations.StringLength(20)]
    public string ChargePointVendor { get; set; }

    [Newtonsoft.Json.JsonProperty("chargePointModel", Required = Newtonsoft.Json.Required.Always)]
    [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
    [System.ComponentModel.DataAnnotations.StringLength(20)]
    public string ChargePointModel { get; set; }

    [Newtonsoft.Json.JsonProperty("chargePointSerialNumber", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
    [System.ComponentModel.DataAnnotations.StringLength(25)]
    public string ChargePointSerialNumber { get; set; }

    [Newtonsoft.Json.JsonProperty("chargeBoxSerialNumber", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
    [System.ComponentModel.DataAnnotations.StringLength(25)]
    public string ChargeBoxSerialNumber { get; set; }

    [Newtonsoft.Json.JsonProperty("firmwareVersion", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
    [System.ComponentModel.DataAnnotations.StringLength(50)]
    public string FirmwareVersion { get; set; }

    [Newtonsoft.Json.JsonProperty("iccid", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
    [System.ComponentModel.DataAnnotations.StringLength(20)]
    public string Iccid { get; set; }

    [Newtonsoft.Json.JsonProperty("imsi", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
    [System.ComponentModel.DataAnnotations.StringLength(20)]
    public string Imsi { get; set; }

    [Newtonsoft.Json.JsonProperty("meterType", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
    [System.ComponentModel.DataAnnotations.StringLength(25)]
    public string MeterType { get; set; }

    [Newtonsoft.Json.JsonProperty("meterSerialNumber", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
    [System.ComponentModel.DataAnnotations.StringLength(25)]
    public string MeterSerialNumber { get; set; }
}