using CPMS.BuildingBlocks.Domain;

namespace CPMS.API.Events.Location;

public class LocationUpdatedEvent : DomainEventBase
{
    public Guid LocationId { get; }
    public string Name { get; }
    public string Address { get; }
    public string City { get; }
    public string PostalCode { get; }
    public string Country { get; }
    public double? Latitude { get; }
    public double? Longitude { get; }
    public string? Description { get; }
    public DateTime UpdatedAt { get; }

    public LocationUpdatedEvent(Guid locationId, string name, string address, 
        string city, string postalCode, string country, double? latitude, 
        double? longitude, string? description, DateTime updatedAt)
    {
        LocationId = locationId;
        Name = name;
        Address = address;
        City = city;
        PostalCode = postalCode;
        Country = country;
        Latitude = latitude;
        Longitude = longitude;
        Description = description;
        UpdatedAt = updatedAt;
    }
}