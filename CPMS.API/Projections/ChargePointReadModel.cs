namespace CPMS.API.Projections;

public class ChargePointReadModel
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public Guid LocationId { get; set; }
    public double? MaxPower { get; set; }
    public double? CurrentPower { get; set; }
    public List<ConnectorReadModel> Connectors { get; set; } = new List<ConnectorReadModel>();
    public List<ConnectorErrorReadModel> ConnectorErrors { get; set; } = new List<ConnectorErrorReadModel>();
    
}