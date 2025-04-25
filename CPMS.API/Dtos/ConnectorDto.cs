namespace CPMS.API.Dtos;

public class ConnectorDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Status { get; set; }
    public DateTime? LastStatusTime { get; set; }
}