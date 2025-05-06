namespace CPMS.API.Projections;

public class ConnectorReadModel
{
    //todo int?
    public Guid ConnectorId { get; set; }
    public string Name { get; set; }
    public string Status { get; set; }
    public DateTime? LastStatusTime { get; set; }
}