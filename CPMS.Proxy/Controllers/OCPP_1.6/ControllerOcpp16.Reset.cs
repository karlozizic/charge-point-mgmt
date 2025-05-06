using CPMS.Proxy.Models;
using CPMS.Proxy.Models.Cpms.Responses;
using CPMS.Proxy.OCPP_1._6;
using Newtonsoft.Json;

namespace CPMS.Proxy.Controllers.OCPP_1._6;

public partial class ControllerOcpp16
{
    private async Task HandleReset(OCPPMessage msgIn, OCPPMessage msgOut)
    {
        ResetRequest resetRequest = JsonConvert.DeserializeObject<ResetRequest>(msgOut.JsonPayload) ?? throw new InvalidOperationException();
        ResetResponse resetResponse = JsonConvert.DeserializeObject<ResetResponse>(msgIn.JsonPayload) ?? throw new InvalidOperationException();
        
        ResetChargerResponse resetChargerResponse = new ResetChargerResponse
        {
            Status = resetResponse.Status.ToString(),
            ChargerId = new Guid(ChargePointStatus.Id),
            Protocol = ChargePointStatus.Protocol
        };
        
        await _cpmsClient.Reset(resetChargerResponse);
        
        Logger.Info($"Reset => Response serialized {resetChargerResponse}");
    }
}