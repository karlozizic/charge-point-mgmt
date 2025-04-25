namespace CPMS.API.Dtos;

public class ChargePointDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public Guid LocationId { get; set; }
    public double? MaxPower { get; set; }
    public double? CurrentPower { get; set; }
    public List<ConnectorDto>? Connectors { get; set; }
}