namespace CPMS.API.Dtos;

public class LocationSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
    public string City { get; set; }
    public string Country { get; set; }
    public int TotalChargePoints { get; set; }
    public int AvailableConnectors { get; set; }
    public DateTime CreatedAt { get; set; }
}