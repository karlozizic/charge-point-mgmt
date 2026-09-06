using CPMS.Proxy.Models;
using CPMS.Proxy.OCPP_1._6;
using Newtonsoft.Json;
using BootNotificationRequest = CPMS.Core.Models.Requests.BootNotificationRequest;

namespace CPMS.Proxy.Controllers.OCPP_1._6;

public partial class ControllerOcpp16
{
    private async Task<string?> HandleBootNotification(OCPPMessage msgIn, OCPPMessage msgOut)
    {
        try
        {
            var boot = JsonConvert.DeserializeObject<Proxy.OCPP_1._6.BootNotificationRequest>(msgIn.JsonPayload)
                       ?? throw new InvalidOperationException("Empty BootNotification payload");

            var registered = await _cpmsClient.BootNotification(new BootNotificationRequest
            {
                OcppChargerId = ChargePointStatus.Id,
                Protocol = ChargePointStatus.Protocol,
                ChargePointVendor = boot.ChargePointVendor,
                ChargePointModel = boot.ChargePointModel,
                ChargePointSerialNumber = boot.ChargePointSerialNumber,
                ChargeBoxSerialNumber = boot.ChargeBoxSerialNumber,
                FirmwareVersion = boot.FirmwareVersion,
                Iccid = boot.Iccid,
                Imsi = boot.Imsi,
                MeterType = boot.MeterType,
                MeterSerialNumber = boot.MeterSerialNumber
            });

            // OCPP 1.6 §4.2: an unknown charger gets a Rejected conf, not a CALLERROR.
            msgOut.JsonPayload = JsonConvert.SerializeObject(new BootNotificationResponse
            {
                CurrentTime = DateTimeOffset.UtcNow,
                Interval = 300,
                Status = registered ? BootNotificationResponseStatus.Accepted : BootNotificationResponseStatus.Rejected
            });

            Logger.Info($"BootNotification => {ChargePointStatus.Id} {(registered ? "accepted" : "rejected: not registered")}");
            return null;
        }
        catch (Exception exp)
        {
            Logger.Error($"BootNotification => Exception: {exp.Message}");
            return ErrorCodes.InternalError;
        }
    }
}
