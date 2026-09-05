namespace CPMS.Core.Models.Requests;

/// <summary>
/// BootNotification.req as forwarded by the gateway. Only vendor and model are required by OCPP 1.6;
/// the rest is optional and must stay nullable, otherwise model validation rejects real chargers.
/// </summary>
public class BootNotificationRequest : BaseMessage
{
    [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
    [System.ComponentModel.DataAnnotations.StringLength(20)]
    public string ChargePointVendor { get; set; }

    [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
    [System.ComponentModel.DataAnnotations.StringLength(20)]
    public string ChargePointModel { get; set; }

    [System.ComponentModel.DataAnnotations.StringLength(25)]
    public string? ChargePointSerialNumber { get; set; }

    [System.ComponentModel.DataAnnotations.StringLength(25)]
    public string? ChargeBoxSerialNumber { get; set; }

    [System.ComponentModel.DataAnnotations.StringLength(50)]
    public string? FirmwareVersion { get; set; }

    [System.ComponentModel.DataAnnotations.StringLength(20)]
    public string? Iccid { get; set; }

    [System.ComponentModel.DataAnnotations.StringLength(20)]
    public string? Imsi { get; set; }

    [System.ComponentModel.DataAnnotations.StringLength(25)]
    public string? MeterType { get; set; }

    [System.ComponentModel.DataAnnotations.StringLength(25)]
    public string? MeterSerialNumber { get; set; }
}
