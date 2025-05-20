using CPMS.Core.Models.Responses;
using CPMS.Proxy.Models;
using CPMS.Proxy.OCPP_1._6;
using Newtonsoft.Json;

namespace CPMS.Proxy.Controllers.OCPP_1._6;

public partial class ControllerOcpp16
{
    private async Task HandleUnlockConnector(OCPPMessage msgIn, OCPPMessage msgOut)
    {
        UnlockConnectorResponse unlockConnectorResponse = JsonConvert.DeserializeObject<UnlockConnectorResponse>(msgIn.JsonPayload) 
                                                          ?? throw new InvalidOperationException();
        
        UnlockConnectorRequest unlockConnectorRequest = JsonConvert.DeserializeObject<UnlockConnectorRequest>(msgOut.JsonPayload) 
                                                        ?? throw new InvalidOperationException();

        ConnectorUnlockedResponse connectorUnlockedChargerResponse = new ConnectorUnlockedResponse
        {
            ChargerId = ChargePointStatus.Id,
            EvseId = 0,
            ConnectorId = unlockConnectorRequest.ConnectorId,
            Status = unlockConnectorResponse.Status.ToString()
        };
        
        await _cpmsClient.ConnectorUnlocked(connectorUnlockedChargerResponse);
    }
}