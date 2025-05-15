using CPMS.Proxy.Models;
using CPMS.Proxy.OCPP_1._6;
using Newtonsoft.Json;
using ClearChargingProfileResponse = CPMS.Core.Models.Responses.ClearChargingProfileResponse;

namespace CPMS.Proxy.Controllers.OCPP_1._6;

public partial class ControllerOcpp16
{
    private async Task HandleClearChargingProfile(OCPPMessage msgIn, OCPPMessage msgOut)
    {
        Proxy.OCPP_1._6.ClearChargingProfileResponse clearChargingProfileResponse = JsonConvert.DeserializeObject<Proxy.OCPP_1._6.ClearChargingProfileResponse>(msgIn.JsonPayload) 
                                                                              ?? throw new InvalidOperationException();

        ClearChargingProfileRequest clearChargingProfileRequest = JsonConvert.DeserializeObject<ClearChargingProfileRequest>(msgOut.JsonPayload) 
                                                                  ?? throw new InvalidOperationException();
        
        ClearChargingProfileResponse clearChargingProfileResponseProxy = new ClearChargingProfileResponse
            {
                ChargerId = new Guid(ChargePointStatus.Id),
                Status = clearChargingProfileResponse.Status.ToString()
            };

        await _cpmsClient.ChargingProfileCleared(clearChargingProfileResponseProxy);
    }
}