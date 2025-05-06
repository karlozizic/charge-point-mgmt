using CPMS.Proxy.Models;
using CPMS.Proxy.Models.Cpms.Requests;
using CPMS.Proxy.OCPP_1._6;
using Newtonsoft.Json;

namespace CPMS.Proxy.Controllers.OCPP_1._6;

public partial class ControllerOcpp16
{
    private async Task<string?> HandleStatusNotification(OCPPMessage msgIn, OCPPMessage msgOut)
    {
        string? errorCode = null;
        bool msgWritten = false;

        try
        {
            StatusNotificationRequest statusNotificationRequest =
                JsonConvert.DeserializeObject<StatusNotificationRequest>(msgIn.JsonPayload)
                ?? throw new InvalidOperationException();

            if (statusNotificationRequest.ConnectorId > 0)
            {
                StatusNotificationCpmsRequest statusNotificationChargerRequest =
                    new StatusNotificationCpmsRequest
                    {
                        OcppChargerId = ChargePointStatus.Id,
                        OcppEvseId = 0,
                        OcppConnectorId = statusNotificationRequest.ConnectorId,
                        LastStatus = statusNotificationRequest.Status.ToString(),
                        LastStatusTime = (statusNotificationRequest.Timestamp ?? DateTimeOffset.UtcNow).DateTime,
                        ChargerId = new Guid(ChargePointStatus.Id),
                        Protocol = ChargePointStatus.Protocol
                    };

                await _cpmsClient.StatusNotification(statusNotificationChargerRequest);
            }
            else
            {
                Logger.Info("Status notification message with connector id 0");
            }

            //msgOut.JsonPayload = JsonConvert.ToString();
        }
        catch (Exception exp)
        {
            Logger.Error($"ChargePoint={ChargePointStatus.Id} / Exception: {exp.Message}", exp);
            errorCode = ErrorCodes.InternalError;
        }
        
        return errorCode;
    }
}