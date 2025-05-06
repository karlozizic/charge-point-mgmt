using CPMS.Proxy.Models;
using CPMS.Proxy.Models.Cpms.Responses;
using CPMS.Proxy.OCPP_1._6;
using Newtonsoft.Json;

namespace CPMS.Proxy.Controllers.OCPP_1._6;

public partial class ControllerOcpp16
{
    private async Task HandleClearChargingProfile(OCPPMessage msgIn, OCPPMessage msgOut)
    {
        ClearChargingProfileResponse clearChargingProfileResponse = JsonConvert.DeserializeObject<ClearChargingProfileResponse>(msgIn.JsonPayload) 
                                                                    ?? throw new InvalidOperationException();

        ClearChargingProfileRequest clearChargingProfileRequest = JsonConvert.DeserializeObject<ClearChargingProfileRequest>(msgOut.JsonPayload) 
                                                                  ?? throw new InvalidOperationException();
        
        ClearChargingProfileCpmsResponse clearChargingProfileResponseProxy = new ClearChargingProfileCpmsResponse
            {
                ChargerId = new Guid(ChargePointStatus.Id),
                Status = clearChargingProfileResponse.Status.ToString()
            };

        await _cpmsClient.ChargingProfileCleared(clearChargingProfileResponseProxy);
    }
}