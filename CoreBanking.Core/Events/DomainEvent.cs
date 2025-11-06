using CoreBanking.Core.Interfaces;
using MediatR;  

namespace CoreBanking.Core.Events;

public abstract record DomainEvent : IDomainEvent, INotification
{
  public Guid EventId { get; init; } = Guid.NewGuid();

  public DateTime OccurredOn { get; init; } = DateTime.UtcNow;

  public string EventType => GetType().Name;
}

