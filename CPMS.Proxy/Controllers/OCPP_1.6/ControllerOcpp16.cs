using CPMS.BuildingBlocks.Infrastructure.Logger;
using CPMS.Proxy.Models;
using CPMS.Proxy.OCPP_1._6;
using CPMS.Proxy.Services;
using Microsoft.Extensions.Configuration;

namespace CPMS.Proxy.Controllers.OCPP_1._6;

public partial class ControllerOcpp16 : BaseController
{
    private readonly ICpmsClient _cpmsClient;
    private readonly IAuthorizationCache _authorizationCache;
    
    public ControllerOcpp16(IConfiguration config, 
        ChargePointStatus chargePointStatus,
        ILoggerService logger,
        ICpmsClient cpmsClient,
        IAuthorizationCache authorizationCache) :
        base(config, chargePointStatus, logger)
    {
        _cpmsClient = cpmsClient;
        _authorizationCache = authorizationCache;
    }

    public override async Task<OCPPMessage> ProcessRequest(OCPPMessage msgIn)
    {
        OCPPMessage msgOut = new OCPPMessage
        {
            MessageType = "3",
            UniqueId = msgIn.UniqueId
        };

        string? errorCode = null;
        
        Logger.Info($"Received message: Action={msgIn.Action}, UniqueId={msgIn.UniqueId}, MessageType={msgIn.MessageType}");

        switch (msgIn.Action)
        {
            case "BootNotification":
                errorCode = await HandleBootNotification(msgIn, msgOut);
                break;

            case "Heartbeat":
                errorCode = HandleHeartBeat(msgIn, msgOut);
                break;

            case "Authorize":
                errorCode = await HandleAuthorize(msgIn, msgOut);
                break;

            case "StartTransaction":
                errorCode = await HandleStartTransaction(msgIn, msgOut);
                break;

            case "StopTransaction":
                errorCode = await HandleStopTransaction(msgIn, msgOut);
                break;

            case "MeterValues":
                errorCode = await HandleMeterValues(msgIn, msgOut);
                break;

            case "StatusNotification":
                errorCode = await HandleStatusNotification(msgIn, msgOut);
                break;

            case "DataTransfer":
                errorCode = HandleDataTransfer(msgIn, msgOut);
                break;
            
            default:
                errorCode = ErrorCodes.NotSupported;
                Logger.Error($"Unknown action: Action={msgIn.Action}");
                break;
        }

        if (!string.IsNullOrEmpty(errorCode))
        {
            // Inavlid message type => return type "4" (CALLERROR)
            msgOut.MessageType = "4";
            msgOut.ErrorCode = errorCode;
            Logger.Info($"Return error code messge: ErrorCode={errorCode}");
        }

        return msgOut;
    }

    public override async Task ProcessAnswer(OCPPMessage msgIn, OCPPMessage msgOut)
    {
        switch (msgOut.Action)
        {
            case "Reset":
                await HandleReset(msgIn, msgOut);
                break;

            case "UnlockConnector":
                await HandleUnlockConnector(msgIn, msgOut);
                break;
                
            case "SetChargingProfile":
                await HandleSetChargingProfile(msgIn, msgOut);
                break;
                
            case "ClearChargingProfile" :
                await HandleClearChargingProfile(msgIn, msgOut);
                break;
                
            default:
                Logger.Error($"Unknown action: Action={msgOut.Action}");
                break;
        }
    }
}