using CPMS.Core.Models.OCPP_1._6;
using CPMS.Core.Models.Responses;
using CPMS.Proxy.Models;
using CPMS.Proxy.OCPP_1._6;
using Newtonsoft.Json;

namespace CPMS.Proxy.Controllers.OCPP_1._6;

public partial class ControllerOcpp16
{
    private async Task<string?> HandleStartTransaction(OCPPMessage msgIn, OCPPMessage msgOut)
    {
        string? errorCode = null;
        try
        {
            StartTransactionRequest startTransactionRequest = JsonConvert.DeserializeObject<StartTransactionRequest>(msgIn.JsonPayload) ?? throw new InvalidOperationException();
            Logger.Info("StartTransaction => Message deserialized");

            if (!_authorizationCache.IsAuthorized(ChargePointStatus.Id, startTransactionRequest.IdTag))
            {
                Logger.Warning($"StartTransaction => Unauthorized attempt with IdTag: {startTransactionRequest.IdTag}");
                StartTransactionResponse unauthorizedResponse = new StartTransactionResponse
                {
                    TransactionId = 0,
                    IdTagInfo = new IdTagInfo
                    {
                        Status = AuthorizationStatus.Invalid
                    }
                };
                
                msgOut.JsonPayload = JsonConvert.SerializeObject(unauthorizedResponse);
                return errorCode;
            }
            
            StartTransactionChargerResponse startTransaction = new()
            {
                OcppChargerId = ChargePointStatus.Id,
                OcppConnectorId = startTransactionRequest.ConnectorId,
                OcppEvseId = 0,
                TimeStart = startTransactionRequest.Timestamp.UtcDateTime,
                MeterStart = (double)startTransactionRequest.MeterStart / 1000,
                IdTag = startTransactionRequest.IdTag
            };
            
            StartTransactionResponse startTransactionResponse = await _cpmsClient.StartTransaction(startTransaction);
            
            StartTransactionResponse startTransactionResponseProxy = new StartTransactionResponse
            {
                TransactionId = startTransactionResponse.TransactionId,
                IdTagInfo = new IdTagInfo
                {
                    ParentIdTag = startTransactionResponse.IdTagInfo.ParentIdTag,
                    Status = startTransactionResponse.IdTagInfo.Status,
                    ExpiryDate = startTransactionResponse.IdTagInfo.ExpiryDate
                }
            };

            msgOut.JsonPayload = JsonConvert.SerializeObject(startTransactionResponseProxy);
            Logger.Info($"StartTransaction => Response serialized {msgOut.JsonPayload}");
        }
        catch (Exception exp)
        {
            Logger.Error($"StartTransaction exception", exp);
            errorCode = ErrorCodes.FormationViolation;
        }

        return errorCode;
    }
}