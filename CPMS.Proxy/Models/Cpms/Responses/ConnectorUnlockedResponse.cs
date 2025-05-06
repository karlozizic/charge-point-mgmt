namespace CPMS.Proxy.Models.Cpms.Responses;

public class ConnectorUnlockedResponse
{
    public string ChargerId { get; set; }
    public int EvseId { get; set; }
    public int ConnectorId { get; set; }
    public string Status { get; set; }   
}