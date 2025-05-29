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


public class LocationMustHaveValidAddressRule : IBusinessRule
{
    private readonly string _address;
    private readonly string _city;
    private readonly string _country;

    public LocationMustHaveValidAddressRule(string address, string city, string country)
    {
        _address = address;
        _city = city;
        _country = country;
    }

    public bool IsBroken() => 
        string.IsNullOrWhiteSpace(_address) || 
        string.IsNullOrWhiteSpace(_city) || 
        string.IsNullOrWhiteSpace(_country);

    public string Message => "Location must have valid address, city, and country.";
}

public class LocationCoordinatesMustBeValidRule : IBusinessRule
{
    private readonly double? _latitude;
    private readonly double? _longitude;

    public LocationCoordinatesMustBeValidRule(double? latitude, double? longitude)
    {
        _latitude = latitude;
        _longitude = longitude;
    }

    public bool IsBroken()
    {
        if (_latitude.HasValue && (_latitude < -90 || _latitude > 90))
            return true;
        
        if (_longitude.HasValue && (_longitude < -180 || _longitude > 180))
            return true;

        return false;
    }

    public string Message => "Location coordinates must be valid (latitude: -90 to 90, longitude: -180 to 180).";
}
