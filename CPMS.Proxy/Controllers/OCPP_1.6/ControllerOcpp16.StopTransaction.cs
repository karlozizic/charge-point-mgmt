using CPMS.Core.Models.OCPP_1._6;
using CPMS.Core.Models.Requests;
using CPMS.Core.Models.Responses;
using CPMS.Proxy.Models;
using CPMS.Proxy.OCPP_1._6;
using Newtonsoft.Json;

namespace CPMS.Proxy.Controllers.OCPP_1._6;

public partial class ControllerOcpp16
{
    private async Task<string?> HandleStopTransaction(OCPPMessage msgIn, OCPPMessage msgOut)
    {
        string? errorCode = null;
        try
        {
            StopTransactionRequest stopTransactionRequest = JsonConvert.DeserializeObject<StopTransactionRequest>(msgIn.JsonPayload) ?? throw new InvalidOperationException(); 
            Logger.Info("StopTransaction => Message deserialized");
            
            StopTransactionCpmsRequest stopTransactionChargerCpmsRequest = new StopTransactionCpmsRequest
            {
                TranscationId = stopTransactionRequest.TransactionId,
                StopTagId = stopTransactionRequest.IdTag,
                MeterStop = (double)stopTransactionRequest.MeterStop / 1000,
                StopReason = stopTransactionRequest.Reason.ToString(),
                TimeStop = stopTransactionRequest.Timestamp.UtcDateTime
            };
            
            StopTransactionResponse stopTransactionResponse = await _cpmsClient.StopTransaction(stopTransactionChargerCpmsRequest);
            StopTransactionResponse stopTransactionResponseProxy = new StopTransactionResponse
            {
                IdTagInfo = new IdTagInfo
                {
                    ParentIdTag = stopTransactionResponse.IdTagInfo.ParentIdTag,
                    ExpiryDate = stopTransactionResponse.IdTagInfo.ExpiryDate,
                    Status = stopTransactionResponse.IdTagInfo.Status
                }
            };
            
            msgOut.JsonPayload = JsonConvert.SerializeObject(stopTransactionResponseProxy);
            Logger.Info($"StopTransaction => Response serialized: {JsonConvert.SerializeObject(stopTransactionResponseProxy)}");
        }
        catch (Exception exp)
        {
            Logger.Error($"Exception", exp);
            errorCode = ErrorCodes.FormationViolation;
        }

        return errorCode;
    }
}