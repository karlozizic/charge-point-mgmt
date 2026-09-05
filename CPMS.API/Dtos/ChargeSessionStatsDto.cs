namespace CPMS.API.Dtos;

public class ChargeSessionStatsDto
{
    public int TotalSessions { get; set; }
    public int ActiveSessions { get; set; }
    public int CompletedSessions { get; set; }
    public double TotalEnergyDelivered { get; set; }
    public double AverageSessionDuration { get; set; }
    public double AverageEnergyPerSession { get; set; }
}
