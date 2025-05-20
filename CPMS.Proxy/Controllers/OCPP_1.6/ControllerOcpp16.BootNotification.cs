using CPMS.Core.Models.Requests;
using CPMS.Proxy.Models;
using CPMS.Proxy.OCPP_1._6;
using Newtonsoft.Json;
using BootNotificationRequest = CPMS.Core.Models.Requests.BootNotificationRequest;

namespace CPMS.Proxy.Controllers.OCPP_1._6;

public partial class ControllerOcpp16
{
    public async Task<string?> HandleBootNotification(OCPPMessage msgIn, OCPPMessage msgOut)
    {
        string? errorCode = null;

        try
        {
            Logger.Info("Processing boot notification...");
            Proxy.OCPP_1._6.BootNotificationRequest bootNotificationRequest = JsonConvert.DeserializeObject<Proxy.OCPP_1._6.BootNotificationRequest>(msgIn.JsonPayload) 
                                                                              ?? throw new InvalidOperationException();
            Logger.Info($"BootNotification => Message deserialized: {bootNotificationRequest}");

            BootNotificationResponse bootNotificationResponse = new BootNotificationResponse
            {
                CurrentTime = DateTimeOffset.UtcNow,
                Interval = 300
            };

            if (ChargePointStatus != null)
            {
                // Known charge station => accept
                bootNotificationResponse.Status = BootNotificationResponseStatus.Accepted;
            }
            else
            {
                // Unknown charge station => reject
                bootNotificationResponse.Status = BootNotificationResponseStatus.Rejected;
                msgOut.JsonPayload = JsonConvert.SerializeObject(bootNotificationResponse);
                return ErrorCodes.FormationViolation;
            }

            msgOut.JsonPayload = JsonConvert.SerializeObject(bootNotificationResponse);
            Logger.Info($"BootNotification => Response serialized: {msgOut.JsonPayload}");

            var bootNotificationCpmsRequest = new BootNotificationRequest
            {
                ChargePointVendor = bootNotificationRequest.ChargePointVendor,
                ChargePointModel = bootNotificationRequest.ChargePointModel,
                ChargePointSerialNumber = bootNotificationRequest.ChargePointSerialNumber,
                ChargeBoxSerialNumber = bootNotificationRequest.ChargeBoxSerialNumber,
                FirmwareVersion = bootNotificationRequest.FirmwareVersion,
                Iccid = bootNotificationRequest.Iccid,
                Imsi = bootNotificationRequest.Imsi,
                OcppChargerId = ChargePointStatus.Id,
                Protocol = ChargePointStatus.Protocol,
                MeterType = bootNotificationRequest.MeterType,
                MeterSerialNumber = bootNotificationRequest.MeterSerialNumber
            };
            
            await _cpmsClient.BootNotification(bootNotificationCpmsRequest);
            Logger.Info($"BootNotification => CPMS request sent: {bootNotificationCpmsRequest}");
            msgOut.JsonPayload = JsonConvert.SerializeObject(bootNotificationCpmsRequest);
        }
        catch (Exception exp)
        {
            Logger.Error($"BootNotification => Exception: {exp.Message}");
            errorCode = ErrorCodes.FormationViolation;
        }

        return errorCode;
    }
}