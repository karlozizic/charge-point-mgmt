namespace CPMS.API.Projections;

public class ConnectorReadModel
{
    public int ConnectorId { get; set; }
    public string Name { get; set; }
    public string Status { get; set; }
    public DateTime? LastStatusTime { get; set; }
    public List<ConnectorErrorReadModel> ConnectorErrors { get; } = new List<ConnectorErrorReadModel>();
    
}