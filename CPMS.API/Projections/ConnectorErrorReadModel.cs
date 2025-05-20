namespace CPMS.API.Projections;

public class ConnectorErrorReadModel
{
    public int ConnectorId { get; set; }
    public string ErrorCode { get; set; }
    public string Info { get; set; }
    public DateTime Timestamp { get; set; }
}