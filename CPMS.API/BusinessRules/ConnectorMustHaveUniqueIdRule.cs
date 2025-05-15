using CPMS.API.Entities;
using CPMS.BuildingBlocks.Domain;

namespace CPMS.API.BusinessRules;

public class ConnectorMustHaveUniqueIdRule : IBusinessRule
{
    private readonly int _connectorId;
    private readonly IEnumerable<Connector> _existingConnectors;
        
    public ConnectorMustHaveUniqueIdRule(int connectorId, IEnumerable<Connector> existingConnectors)
    {
        _connectorId = connectorId;
        _existingConnectors = existingConnectors;
    }
        
    public bool IsBroken() => _existingConnectors.Any(c => c.Id == _connectorId);
        
    public string Message => $"Connector with given id already exists: {_connectorId}";
}