using CPMS.BuildingBlocks.Infrastructure.Logger;
using CPMS.Proxy.Models;
using CPMS.Proxy.OCPP_1._6;
using CPMS.Proxy.Services;

namespace CPMS.Proxy.Controllers.OCPP_1._6;

/// <summary>
/// Handles one OCPP 1.6 CALL from a connected charger. Each action lives in its own partial file and
/// returns an error code (CALLERROR) or null (CALLRESULT with the payload it wrote to msgOut).
/// </summary>
public partial class ControllerOcpp16
{
    private readonly ICpmsClient _cpmsClient;
    private readonly IAuthorizationCache _authorizationCache;

    private ChargePointStatus ChargePointStatus { get; }
    private ILoggerService Logger { get; }

    public ControllerOcpp16(
        ChargePointStatus chargePointStatus,
        ILoggerService logger,
        ICpmsClient cpmsClient,
        IAuthorizationCache authorizationCache)
    {
        ChargePointStatus = chargePointStatus;
        Logger = logger;
        _cpmsClient = cpmsClient;
        _authorizationCache = authorizationCache;
    }

    public async Task<OCPPMessage> ProcessRequest(OCPPMessage msgIn)
    {
        var msgOut = new OCPPMessage
        {
            MessageType = "3",
            UniqueId = msgIn.UniqueId
        };

        Logger.Info($"Received {msgIn.Action} (UniqueId={msgIn.UniqueId}) from {ChargePointStatus.Id}");

        var errorCode = msgIn.Action switch
        {
            "BootNotification" => await HandleBootNotification(msgIn, msgOut),
            "Heartbeat" => HandleHeartBeat(msgIn, msgOut),
            "Authorize" => await HandleAuthorize(msgIn, msgOut),
            "StartTransaction" => await HandleStartTransaction(msgIn, msgOut),
            "StopTransaction" => await HandleStopTransaction(msgIn, msgOut),
            "MeterValues" => await HandleMeterValues(msgIn, msgOut),
            "StatusNotification" => await HandleStatusNotification(msgIn, msgOut),
            "DataTransfer" => HandleDataTransfer(msgIn, msgOut),
            _ => NotSupported(msgIn.Action)
        };

        if (!string.IsNullOrEmpty(errorCode))
        {
            msgOut.MessageType = "4";
            msgOut.ErrorCode = errorCode;
            Logger.Info($"Returning CALLERROR {errorCode} for {msgIn.Action}");
        }

        return msgOut;
    }

    private string NotSupported(string action)
    {
        Logger.Error($"Unknown action: {action}");
        return ErrorCodes.NotSupported;
    }
}
