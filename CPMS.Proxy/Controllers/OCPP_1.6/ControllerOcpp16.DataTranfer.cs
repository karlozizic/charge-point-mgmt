using CPMS.Proxy.Models;
using CPMS.Proxy.OCPP_1._6;
using Newtonsoft.Json;

namespace CPMS.Proxy.Controllers.OCPP_1._6;

public partial class ControllerOcpp16
{
    private string? HandleDataTransfer(OCPPMessage msgIn, OCPPMessage msgOut)
    {
        string? errorCode = null;
        DataTransferResponse dataTransferResponse = new DataTransferResponse();

        bool msgWritten = false;

        try
        {
            DataTransferRequest dataTransferRequest = JsonConvert.DeserializeObject<DataTransferRequest>(msgIn.JsonPayload) ?? throw new InvalidOperationException();

            if (ChargePointStatus != null)
            {
                // Known charge station
                Logger.Info($"Incoming data transfer request from {ChargePointStatus.Id} with VendorId={dataTransferRequest.VendorId} / MessageId={dataTransferRequest.MessageId} / Data={dataTransferRequest.Data}");
                dataTransferResponse.Status = DataTransferResponseStatus.Accepted;
            }
            else
            {
                // Unknown charge station
                errorCode = ErrorCodes.GenericError;
                dataTransferResponse.Status = DataTransferResponseStatus.Rejected;
            }

            msgOut.JsonPayload = JsonConvert.SerializeObject(dataTransferResponse);
            Logger.Info($"DataTransfer => Response serialized: {msgOut.JsonPayload}");
        }
        catch (Exception exp)
        {
            Logger.Error("DataTransfer => Exception: {0}", exp);
            errorCode = ErrorCodes.InternalError;
        }
            
        return errorCode;
    }
}