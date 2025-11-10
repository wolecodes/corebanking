
namespace CoreBanking.Core.Interfaces;

public interface IDomainEvent
{
    DateTime OccurredOn { get; }
    Guid EventId { get; }
    string EventType { get; }
}

