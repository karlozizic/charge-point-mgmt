using CPMS.API.Events.ChargeSession;
using JasperFx.Events;
using Marten.Events.Projections;

namespace CPMS.API.Projections;

public partial class MeterValueProjection : EventProjection
{
    public MeterValueReadModel Create(IEvent<MeterValueRecordedEvent> @event) => new()
    {
        Id = @event.Id,
        SessionId = @event.Data.ChargeSessionId,
        TransactionId = @event.Data.TransactionId,
        CurrentPower = @event.Data.CurrentPower,
        EnergyConsumed = @event.Data.EnergyConsumed,
        StateOfCharge = @event.Data.StateOfCharge,
        Timestamp = @event.Data.Timestamp
    };
}
