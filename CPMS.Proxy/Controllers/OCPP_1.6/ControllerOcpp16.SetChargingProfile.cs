using CPMS.Proxy.Models;
using CPMS.Proxy.Models.Cpms.Responses;
using CPMS.Proxy.OCPP_1._6;
using Newtonsoft.Json;

namespace CPMS.Proxy.Controllers.OCPP_1._6;

public partial class ControllerOcpp16
{
    private async Task HandleSetChargingProfile(OCPPMessage msgIn, OCPPMessage msgOut)
    {
        SetChargingProfileResponse setChargingProfileResponse =
            JsonConvert.DeserializeObject<SetChargingProfileResponse>(msgIn.JsonPayload) ??
            throw new InvalidOperationException();
        
        SetChargingProfileRequest changeConfigurationRequest =
            JsonConvert.DeserializeObject<SetChargingProfileRequest>(msgOut.JsonPayload) 
            ?? throw new InvalidOperationException();
        
        SetChargingProfileCpmsResponse setChargingProfileResponseProxy = new SetChargingProfileCpmsResponse
            {
                ChargerId = new Guid(ChargePointStatus.Id),
                Status = setChargingProfileResponse.Status.ToString()
            };

        await _cpmsClient.ChargingProfileSet(setChargingProfileResponseProxy);
    }
}