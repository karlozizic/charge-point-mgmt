using CPMS.BuildingBlocks.Domain;

namespace CPMS.API.BusinessRules;

public class ConnectorMustExistRule : IBusinessRule
{
    private readonly Guid _connectorId;
        
    public ConnectorMustExistRule(Guid connectorId)
    {
        _connectorId = connectorId;
    }
        
    public bool IsBroken() => true; 
        
    public string Message => $"Connector {_connectorId} does not exist";
}