namespace CPMS.API.Dtos;

public class LocationDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
    public string City { get; set; }
    public string PostalCode { get; set; }
    public string Country { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<ChargePointSummaryDto>? ChargePoints { get; set; }
    public int TotalChargePoints { get; set; }
    public int AvailableConnectors { get; set; }
}