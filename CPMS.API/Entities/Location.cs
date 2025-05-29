using CPMS.API.BusinessRules;
using CPMS.API.Events.Location;
using CPMS.BuildingBlocks.Domain;

namespace CPMS.API.Entities;

public class Location : Entity, IAggregateRoot
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Address { get; private set; }
    public string City { get; private set; }
    public string PostalCode { get; private set; }
    public string Country { get; private set; }
    public double? Latitude { get; private set; }
    public double? Longitude { get; private set; }
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    
    private readonly List<Guid> _chargePointIds = new List<Guid>();
    public IReadOnlyCollection<Guid> ChargePointIds => _chargePointIds.AsReadOnly();

    private Location()
    {
    }

    public Location(Guid id, string name, string address, string city, 
        string postalCode, string country, double? latitude, double? longitude, 
        string? description)
    {
        CheckRule(new LocationMustHaveValidAddressRule(address, city, country));
        CheckRule(new LocationCoordinatesMustBeValidRule(latitude, longitude));

        var @event = new LocationCreatedEvent(
            id, name, address, city, postalCode, country, 
            latitude, longitude, description, DateTime.UtcNow);

        AddDomainEvent(@event);
        Apply(@event);
    }
    
    private void Apply(LocationCreatedEvent @event)
    {
        Id = @event.LocationId;
        Name = @event.Name;
        Address = @event.Address;
        City = @event.City;
        PostalCode = @event.PostalCode;
        Country = @event.Country;
        Latitude = @event.Latitude;
        Longitude = @event.Longitude;
        Description = @event.Description;
        CreatedAt = @event.CreatedAt;
        IsActive = true;
    }
    
    public void UpdateDetails(string name, string address, string city, 
        string postalCode, string country, double? latitude, double? longitude, 
        string? description)
    {
        CheckRule(new LocationMustHaveValidAddressRule(address, city, country));
        CheckRule(new LocationCoordinatesMustBeValidRule(latitude, longitude));

        var @event = new LocationUpdatedEvent(
            Id, name, address, city, postalCode, country, 
            latitude, longitude, description, DateTime.UtcNow);

        AddDomainEvent(@event);
        Apply(@event);
    }

    private void Apply(LocationUpdatedEvent @event)
    {
        Name = @event.Name;
        Address = @event.Address;
        City = @event.City;
        PostalCode = @event.PostalCode;
        Country = @event.Country;
        Latitude = @event.Latitude;
        Longitude = @event.Longitude;
        Description = @event.Description;
    }
    
    public void AddChargePoint(Guid chargePointId, string ocppChargerId)
    {
        if (_chargePointIds.Contains(chargePointId))
            return;

        var @event = new ChargePointAddedToLocationEvent(
            Id, chargePointId, ocppChargerId, DateTime.UtcNow);
        
        AddDomainEvent(@event);
        Apply(@event);
    }

    private void Apply(ChargePointAddedToLocationEvent @event)
    {
        _chargePointIds.Add(@event.ChargePointId);
    }
}