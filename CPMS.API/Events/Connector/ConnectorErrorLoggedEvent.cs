using CPMS.API.Projections;
using CPMS.BuildingBlocks.Domain;

namespace CPMS.API.Events.Connector;

public class ConnectorError
{
    public string ErrorCode { get; private set; }
    public string Info { get; private set; }
    public DateTime Timestamp { get; private set; }

    public ConnectorError(string errorCode, string info, DateTime timestamp)
    {
        ErrorCode = errorCode;
        Info = info;
        Timestamp = timestamp;
    }
}


public class ConnectorErrorLoggedEvent : DomainEventBase
{
    public Guid ChargePointId { get; }
    public int ConnectorId { get; }
    public string ErrorCode { get; }
    public string Info { get; }
    public DateTime Timestamp { get; }

    public ConnectorErrorLoggedEvent(
        Guid chargePointId,
        int connectorId,
        string errorCode,
        string info,
        DateTime timestamp)
    {
        ChargePointId = chargePointId;
        ConnectorId = connectorId;
        ErrorCode = errorCode;
        Info = info;
        Timestamp = timestamp;
    }
}