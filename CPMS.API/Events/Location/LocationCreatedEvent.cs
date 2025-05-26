using CPMS.BuildingBlocks.Domain;

namespace CPMS.API.Events.Location;

public class LocationCreatedEvent : DomainEventBase
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
    public DateTime CreatedAt { get; }

    public LocationCreatedEvent(Guid locationId, string name, string address, 
        string city, string postalCode, string country, double? latitude, 
        double? longitude, string? description, DateTime createdAt)
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
        CreatedAt = createdAt;
    }
}