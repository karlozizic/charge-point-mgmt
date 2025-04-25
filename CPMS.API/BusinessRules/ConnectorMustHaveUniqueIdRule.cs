using CPMS.API.Entities;
using CPMS.BuildingBlocks.Domain;

namespace CPMS.API.BusinessRules;

public class ConnectorMustHaveUniqueIdRule : IBusinessRule
{
    private readonly Guid _connectorId;
    private readonly IEnumerable<Connector> _existingConnectors;
        
    public ConnectorMustHaveUniqueIdRule(Guid connectorId, IEnumerable<Connector> existingConnectors)
    {
        _connectorId = connectorId;
        _existingConnectors = existingConnectors;
    }
        
    public bool IsBroken() => _existingConnectors.Any(c => c.Id == _connectorId);
        
    public string Message => $"Connector with given id already exists: {_connectorId}";
}