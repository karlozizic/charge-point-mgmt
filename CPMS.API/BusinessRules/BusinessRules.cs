using CPMS.API.Entities;
using CPMS.BuildingBlocks.Domain;

namespace CPMS.API.BusinessRules;

public class CannotAddMeterValuesAfterStopRule : IBusinessRule
{
    public bool IsBroken() => true;

    public string Message => "Cannot add meter values after stop";
}

public class CannotStopAlreadyStoppedSessionRule : IBusinessRule
{
    public bool IsBroken() => true;

    public string Message => "Cannot stop an already stopped session.";
}

public class ChargePointMustHaveValidNameRule : IBusinessRule
{
    private readonly string _name;
    
    public ChargePointMustHaveValidNameRule(string name)
    {
        _name = name;
    }

    public bool IsBroken() => string.IsNullOrWhiteSpace(_name);
    public string Message => "Charge point must have valid name!";
}

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

public class TagNotValidRule : IBusinessRule
{
    private readonly string _name;
    
    public TagNotValidRule(string name)
    {
        _name = name;
    }
   
    public bool IsBroken() => true;

    public string Message => $"Tag {_name} is not valid";
}
