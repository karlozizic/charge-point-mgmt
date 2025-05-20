using CPMS.BuildingBlocks.Domain;

namespace CPMS.API.BusinessRules;

public class ConnectorMustExistRule : IBusinessRule
{
    private readonly int _connectorId;
        
    public ConnectorMustExistRule(int connectorId)
    {
        _connectorId = connectorId;
    }
        
    public bool IsBroken() => true; 
        
    public string Message => $"Connector {_connectorId} does not exist";
}