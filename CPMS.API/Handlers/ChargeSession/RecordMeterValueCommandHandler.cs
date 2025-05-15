using CPMS.API.Repositories;
using CPMS.Core.Models.Requests;
using MediatR;

namespace CPMS.API.Handlers.ChargeSession;

public class MeterValuesCommand : IRequest
{
    public Guid ChargerId { get; set; }
    public int TransactionId { get; set; }
    public double? CurrentPower { get; private set; }
    public double? EnergyConsumed { get; private set; }
    public DateTimeOffset? MeterTime { get; private set; }
    public double? StateOfCharge { get; private set; }
    
    public MeterValuesCommand(MeterValuesRequest request)
    {
        ChargerId = request.ChargerId;
        TransactionId = request.TransactionId;
        CurrentPower = request.CurrentPower;
        EnergyConsumed = request.EnergyConsumed;
        MeterTime = request.MeterTime ?? DateTimeOffset.UtcNow;
        StateOfCharge = request.StateOfCharge;
    }
}

public class RecordMeterValuerequestHandler : IRequestHandler<MeterValuesCommand>
{
    private readonly IChargeSessionRepository _chargeSessionRepository;
    private readonly ILogger<RecordMeterValuerequestHandler> _logger;
        
    public RecordMeterValuerequestHandler(
        IChargeSessionRepository chargeSessionRepository,
        ILogger<RecordMeterValuerequestHandler> logger)
    {
        _chargeSessionRepository = chargeSessionRepository;
        _logger = logger;
    }
    
    public async Task Handle(MeterValuesCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var chargeSession = await _chargeSessionRepository.GetByTransactionIdAsync(request.TransactionId);
                
            if (chargeSession == null)
            {
                _logger.LogWarning($"Received meter value for unknown transaction: {request.TransactionId}");
                return;
            }
                
            chargeSession.AddMeterValue(request, DateTime.UtcNow);
                
            await _chargeSessionRepository.UpdateAsync(chargeSession);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error recording meter value for transaction {request.TransactionId}");
            throw;
        }
    }
}