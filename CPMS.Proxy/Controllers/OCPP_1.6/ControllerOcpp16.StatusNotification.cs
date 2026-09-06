using CPMS.Proxy.Models;
using CPMS.Proxy.OCPP_1._6;
using Newtonsoft.Json;
using StatusNotificationRequest = CPMS.Core.Models.Requests.StatusNotificationRequest;

namespace CPMS.Proxy.Controllers.OCPP_1._6;

public partial class ControllerOcpp16
{
    private async Task<string?> HandleStatusNotification(OCPPMessage msgIn, OCPPMessage msgOut)
    {
        try
        {
            var status = JsonConvert.DeserializeObject<Proxy.OCPP_1._6.StatusNotificationRequest>(msgIn.JsonPayload)
                         ?? throw new InvalidOperationException("Empty StatusNotification payload");

            // Connector 0 is the charge point itself; only connector statuses are forwarded.
            if (status.ConnectorId > 0)
            {
                await _cpmsClient.StatusNotification(new StatusNotificationRequest
                {
                    OcppChargerId = ChargePointStatus.Id,
                    Protocol = ChargePointStatus.Protocol,
                    OcppEvseId = 0,
                    OcppConnectorId = status.ConnectorId,
                    LastStatus = status.Status.ToString(),
                    LastStatusTime = (status.Timestamp ?? DateTimeOffset.UtcNow).UtcDateTime
                });
            }

            msgOut.JsonPayload = JsonConvert.SerializeObject(new Proxy.OCPP_1._6.StatusNotificationResponse());
            return null;
        }
        catch (JsonException exp)
        {
            Logger.Warning($"StatusNotification => malformed payload: {exp.Message}");
            return ErrorCodes.FormationViolation;
        }
        catch (Exception exp)
        {
            Logger.Error($"StatusNotification => Exception: {exp.Message}", exp);
            return ErrorCodes.InternalError;
        }
    }
}
