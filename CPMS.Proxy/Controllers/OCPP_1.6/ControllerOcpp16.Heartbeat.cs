using CPMS.Proxy.Models;
using CPMS.Proxy.OCPP_1._6;
using Newtonsoft.Json;

namespace CPMS.Proxy.Controllers.OCPP_1._6;

public partial class ControllerOcpp16 
{
    private string? HandleHeartBeat(OCPPMessage msgIn, OCPPMessage msgOut)
    {
        string? errorCode = null;

        HeartbeatResponse heartbeatResponse = new HeartbeatResponse
        {
            CurrentTime = DateTimeOffset.UtcNow
        };
        msgOut.JsonPayload = JsonConvert.SerializeObject(heartbeatResponse);

        Logger.Info($"Heartbeat processed for charger id: {ChargePointStatus.Id}");

        return errorCode;
    }
}