namespace CPMS.BuildingBlocks.Domain;

public class DomainEventBase : IDomainEvent
{
    public Guid Id { get; }

    public DateTime Timestamp { get; }

    public DomainEventBase()
    {
        this.Id = Guid.NewGuid();
        this.Timestamp = DateTime.UtcNow;
    }
}