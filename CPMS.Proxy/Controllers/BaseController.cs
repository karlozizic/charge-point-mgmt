using CPMS.BuildingBlocks.Infrastructure.Logger;
using CPMS.Proxy.Models;
using Microsoft.Extensions.Configuration;

namespace CPMS.Proxy.Controllers;

public abstract class BaseController
{
    protected IConfiguration Configuration { get; set; }
    protected ChargePointStatus ChargePointStatus { get; set; }
    protected ILoggerService Logger { get; set; }

    public BaseController(IConfiguration config,
        ChargePointStatus chargePointStatus,
        ILoggerService logger)
    {
        Configuration = config;
        Logger = logger;

        if (chargePointStatus != null)
        {
            ChargePointStatus = chargePointStatus;
        }
        else
        {
            Logger.Error("No charge point status found");
        }
    }

    public abstract Task<OCPPMessage> ProcessRequest(OCPPMessage msgIn);
    public abstract Task ProcessAnswer(OCPPMessage msgIn, OCPPMessage msgOut);

    //todo
    /*public bool UpdateConnectorStatus(int connectorId, string status, 
        DateTimeOffset? statusTime, double? meter, DateTimeOffset? meterTime)
    {
        try
        {
            ConnectorStatus connectorStatus = DbContext.Find<ConnectorStatus>(ChargePointStatus.Id, connectorId);
            if (connectorStatus == null)
            {
                // no matching entry => create connector status
                connectorStatus = new ConnectorStatus();
                connectorStatus.ChargePointId = ChargePointStatus.Id;
                connectorStatus.ConnectorId = connectorId;
                Logger.LogTrace("UpdateConnectorStatus => Creating new DB-ConnectorStatus: ID={0} / Connector={1}", connectorStatus.ChargePointId, connectorStatus.ConnectorId);
                DbContext.Add<ConnectorStatus>(connectorStatus);
            }

            if (!string.IsNullOrEmpty(status))
            {
                connectorStatus.LastStatus = status;
                connectorStatus.LastStatusTime = ((statusTime.HasValue) ? statusTime.Value : DateTimeOffset.UtcNow).DateTime;
            }

            if (meter.HasValue)
            {
                connectorStatus.LastMeter = meter.Value;
                connectorStatus.LastMeterTime = ((meterTime.HasValue) ? meterTime.Value : DateTimeOffset.UtcNow).DateTime;
            }
            DbContext.SaveChanges();
            Logger.LogInformation("UpdateConnectorStatus => Save ConnectorStatus: ID={0} / Connector={1} / Status={2} / Meter={3}", connectorStatus.ChargePointId, connectorId, status, meter);
            return true;
        }
        catch (Exception exp)
        {
            Logger.LogError(exp, "UpdateConnectorStatus => Exception writing connector status (ID={0} / Connector={1}): {2}", ChargePointStatus?.Id, connectorId, exp.Message);
        }

        return false;
    }*/
    
    //todo
    /*protected static string CleanChargeTagId(string rawChargeTagId, ILogger logger)
    {
        string idTag = rawChargeTagId;

        // KEBA adds the serial to the idTag ("<idTag>_<serial>") => cut off suffix
        if (!string.IsNullOrWhiteSpace(rawChargeTagId))
        {
            int sep = rawChargeTagId.IndexOf('_');
            if (sep >= 0)
            {
                idTag = rawChargeTagId.Substring(0, sep);
                logger.LogTrace("CleanChargeTagId => Charge tag '{0}' => '{1}'", rawChargeTagId, idTag);
            }
        }

        return idTag;
    }*/

    protected static DateTimeOffset MaxExpiryDate => DateTime.UtcNow.Date.AddYears(1);
}