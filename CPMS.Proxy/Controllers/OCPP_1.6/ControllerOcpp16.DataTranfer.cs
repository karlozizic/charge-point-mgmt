using CPMS.Proxy.Models;
using CPMS.Proxy.OCPP_1._6;
using Newtonsoft.Json;

namespace CPMS.Proxy.Controllers.OCPP_1._6;

public partial class ControllerOcpp16
{
    private string? HandleDataTransfer(OCPPMessage msgIn, OCPPMessage msgOut)
    {
        try
        {
            var request = JsonConvert.DeserializeObject<DataTransferRequest>(msgIn.JsonPayload)
                          ?? throw new InvalidOperationException("Empty DataTransfer payload");

            // Vendor-specific data is accepted and logged; nothing consumes it yet.
            Logger.Info($"DataTransfer from {ChargePointStatus.Id}: VendorId={request.VendorId} / MessageId={request.MessageId} / Data={request.Data}");

            msgOut.JsonPayload = JsonConvert.SerializeObject(new DataTransferResponse
            {
                Status = DataTransferResponseStatus.Accepted
            });
            return null;
        }
        catch (Exception exp)
        {
            Logger.Error($"DataTransfer => Exception: {exp.Message}");
            return ErrorCodes.InternalError;
        }
    }
}
