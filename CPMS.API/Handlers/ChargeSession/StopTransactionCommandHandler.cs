using CPMS.API.Repositories;
using CPMS.Core.Models.Enums;
using CPMS.Core.Models.OCPP_1._6;
using CPMS.Core.Models.Responses;
using MediatR;

namespace CPMS.API.Handlers.ChargeSession;

public class StopTransactionCommand : IRequest<StopTransactionResponse>
{
    public int TranscationId { get; set; }
    public double? MeterStop { get; set; }
    public DateTime? TimeStop { get; set; }
    public string? StopTagId { get; set; }
    public string? StopReason { get; set; }

    public StopTransactionCommand(
        int transactionId,
        DateTime? timeStop,
        double? meterStop,
        string? tagId,
        string? reason)
    {
        TranscationId = transactionId;
        TimeStop = timeStop;
        MeterStop = meterStop;
        StopTagId = tagId;
        StopReason = reason;
    }
}

public class StopTransactionCommandHandler : IRequestHandler<StopTransactionCommand, StopTransactionResponse>
{
    private readonly IChargeSessionRepository _chargeSessionRepository;
    private readonly IChargePointRepository _chargePointRepository;
    private readonly ILogger<StopTransactionCommandHandler> _logger;
    
    public StopTransactionCommandHandler(
        IChargeSessionRepository chargeSessionRepository,
        IChargePointRepository chargePointRepository,
        ILogger<StopTransactionCommandHandler> logger)
    {
        _chargeSessionRepository = chargeSessionRepository;
        _chargePointRepository = chargePointRepository;
        _logger = logger;
    }
    
    public async Task<StopTransactionResponse> Handle(StopTransactionCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var chargeSession = await _chargeSessionRepository.GetByTransactionIdAsync(command.TranscationId);
            
            if (chargeSession == null)
            {
                _logger.LogWarning($"Received stop transaction for unknown transaction: {command.TranscationId}");
                return new StopTransactionResponse
                {
                    IdTagInfo = new IdTagInfo { Status = AuthorizationStatus.Invalid }
                };
            }
            
            if (command.StopTagId == null || command.MeterStop == null)
                throw new ArgumentNullException(nameof(command), "StopTagId and MeterStop cannot be null");
            
            chargeSession.StopCharging(command.StopTagId, command.MeterStop.Value, command.StopReason);
            
            await _chargeSessionRepository.UpdateAsync(chargeSession);
            
            var chargePoint = await _chargePointRepository.GetByIdAsync(chargeSession.ChargePointId);
            
            if (chargePoint != null)
            {
                var connector = chargePoint.Connectors.FirstOrDefault(c => c.Id == chargeSession.ConnectorId);
                    
                if (connector != null)
                {
                    chargePoint.UpdateConnectorStatus(connector.Id, "Available");
                    await _chargePointRepository.UpdateAsync(chargePoint);
                }

                return new StopTransactionResponse
                {
                    IdTagInfo = new IdTagInfo{ Status = AuthorizationStatus.Accepted}
                };
            }

            return new StopTransactionResponse
            {
                IdTagInfo = new IdTagInfo { Status = AuthorizationStatus.Invalid }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error stopping transaction {command.TranscationId}");
            throw;
        }
    }
}