using CPMS.Core.Models.OCPP_1._6;
using CPMS.Core.Models.Requests;
using CPMS.Core.Models.Responses;
using CPMS.Proxy.Models;
using CPMS.Proxy.OCPP_1._6;
using Newtonsoft.Json;

namespace CPMS.Proxy.Controllers.OCPP_1._6;

public partial class ControllerOcpp16
{
    private async Task<string?> HandleAuthorize(OCPPMessage msgIn, OCPPMessage msgOut)
    {
        string? errorCode = null;

        try
        {
            AuthorizeRequest authorizeRequest = JsonConvert.DeserializeObject<AuthorizeRequest>(msgIn.JsonPayload) 
                                                ?? throw new InvalidOperationException($"Deserialization error: Message {msgIn.JsonPayload}");
            
            AuthorizeResponse authorizeResponse = new AuthorizeResponse
            {
                IdTagInfo = new IdTagInfo()
            };

            AuthorizeChargerRequest authorizeChargerRequest = new AuthorizeChargerRequest
            {
                IdTag = authorizeRequest.IdTag,
                ChargerId = new Guid(ChargePointStatus.Id),
                Protocol = ChargePointStatus.Protocol
            };

            AuthorizeChargerResponse authorizationChargerResponse = await _cpmsClient.Authorize(authorizeChargerRequest);

            authorizeResponse.IdTagInfo.Status = authorizationChargerResponse.AuthorizationStatus;
            msgOut.JsonPayload = JsonConvert.SerializeObject(authorizeResponse);
        }
        catch (Exception exp)
        {
            Logger.Error($"Authorize => Exception: {exp.Message}");
            errorCode = ErrorCodes.FormationViolation;
        }
        
        return errorCode;
    }
}