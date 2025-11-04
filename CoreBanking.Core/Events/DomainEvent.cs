using CoreBanking.Core.Interfaces;
using CoreBanking.Core.ValueObjects;

namespace CoreBanking.Core.Events;



public abstract record DomainEvent : IDomainEvent
{
  public Guid EventId { get; init; } = Guid.NewGuid();

  public DateTime OccurredOn { get; init; } = DateTime.UtcNow;

  public string EventType => GetType().Name;
}

