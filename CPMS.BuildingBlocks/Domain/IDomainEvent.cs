using MediatR;

namespace CPMS.BuildingBlocks.Domain;

public interface IDomainEvent : INotification
{
    Guid Id { get; }

    DateTime Timestamp { get; }
}