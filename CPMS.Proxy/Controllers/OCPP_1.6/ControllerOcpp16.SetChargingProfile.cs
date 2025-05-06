using CPMS.Core.Models.Responses;
using CPMS.Proxy.Models;
using CPMS.Proxy.OCPP_1._6;
using Newtonsoft.Json;
using SetChargingProfileResponse = CPMS.Core.Models.Responses.SetChargingProfileResponse;

namespace CPMS.Proxy.Controllers.OCPP_1._6;

public partial class ControllerOcpp16
{
    private async Task HandleSetChargingProfile(OCPPMessage msgIn, OCPPMessage msgOut)
    {
        Proxy.OCPP_1._6.SetChargingProfileResponse setChargingProfileResponse =
            JsonConvert.DeserializeObject<Proxy.OCPP_1._6.SetChargingProfileResponse>(msgIn.JsonPayload) ??
            throw new InvalidOperationException();
        
        SetChargingProfileRequest changeConfigurationRequest =
            JsonConvert.DeserializeObject<SetChargingProfileRequest>(msgOut.JsonPayload) 
            ?? throw new InvalidOperationException();
        
        SetChargingProfileResponse setChargingProfileResponseProxy = new SetChargingProfileResponse
            {
                ChargerId = new Guid(ChargePointStatus.Id),
                Status = setChargingProfileResponse.Status.ToString()
            };

        await _cpmsClient.ChargingProfileSet(setChargingProfileResponseProxy);
    }
}