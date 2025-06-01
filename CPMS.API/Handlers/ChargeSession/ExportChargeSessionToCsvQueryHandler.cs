using System.Text;
using CPMS.API.Projections;
using Marten;
using MediatR;

namespace CPMS.API.Handlers.ChargeSession;

public class ExportChargeSessionToCsvQuery : IRequest<string>
{
    public Guid SessionId { get; set; }
}

public class ExportChargeSessionToCsvQueryHandler : IRequestHandler<ExportChargeSessionToCsvQuery, string>
{
    private readonly IQuerySession _querySession;

    public ExportChargeSessionToCsvQueryHandler(IQuerySession querySession)
    {
        _querySession = querySession;
    }

    public async Task<string> Handle(ExportChargeSessionToCsvQuery request, CancellationToken cancellationToken)
    {
        var session = await _querySession.Query<ChargeSessionReadModel>()
            .FirstOrDefaultAsync(s => s.Id == request.SessionId, cancellationToken);

        if (session == null)
            throw new InvalidOperationException($"Session with ID {request.SessionId} not found.");

        return GenerateCsvReport(session);
    }

    private string GenerateCsvReport(ChargeSessionReadModel session)
    {
        var csv = new StringBuilder();
        
        csv.AppendLine("Electrify CPMS - Charge Session Report");
        csv.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        csv.AppendLine();
        
        csv.AppendLine("SESSION DETAILS");
        csv.AppendLine("Field,Value");
        csv.AppendLine($"Transaction ID,{session.TransactionId}");
        csv.AppendLine($"Tag ID,{session.TagId}");
        csv.AppendLine($"ChargePoint ID,{session.ChargePointId}");
        csv.AppendLine($"Connector ID,{session.ConnectorId}");
        csv.AppendLine($"Start Time,{session.StartTime:yyyy-MM-dd HH:mm:ss}");
        csv.AppendLine($"Stop Time,{(session.StopTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "Ongoing")}");
        csv.AppendLine($"Duration,{GetDuration(session)}");
        csv.AppendLine($"Start Meter (Wh),{session.StartMeterValue:F2}");
        csv.AppendLine($"Stop Meter (Wh),{(session.StopMeterValue?.ToString("F2") ?? "N/A")}");
        csv.AppendLine($"Energy Delivered (kWh),{(session.EnergyDeliveredKWh ?? 0):F2}");
        csv.AppendLine($"Status,{session.Status}");
        csv.AppendLine($"Stop Reason,{session.StopReason}");
        
        if (session.MeterValues?.Any() == true)
        {
            csv.AppendLine();
            csv.AppendLine("METER READINGS");
            csv.AppendLine("Timestamp,Power (kW),Energy (kWh),SoC (%)");
            
            foreach (var reading in session.MeterValues.OrderBy(m => m.Timestamp))
            {
                csv.AppendLine($"{reading.Timestamp:yyyy-MM-dd HH:mm:ss}," +
                             $"{reading.CurrentPower?.ToString("F1") ?? "-"}," +
                             $"{reading.EnergyConsumed?.ToString("F3") ?? "-"}," +
                             $"{reading.StateOfCharge?.ToString("F1") ?? "-"}");
            }
        }
        
        return csv.ToString();
    }

    private string GetDuration(ChargeSessionReadModel session)
    {
        if (!session.StopTime.HasValue)
            return "Session ongoing";
            
        var duration = session.StopTime.Value - session.StartTime;
        var hours = (int)duration.TotalHours;
        var minutes = duration.Minutes;
        
        return $"{hours}h {minutes}m";
    }
}