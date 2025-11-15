using CoreBanking.Core.Events;

namespace CoreBanking.Core.Common;

public abstract class AggregateRoot<TId> where TId : notnull
{
  private readonly List<DomainEvent> _domainEvents = new();

  public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

  protected void AddDomainEvent(DomainEvent domainEvent)
  {
    _domainEvents.Add(domainEvent);
  }

  public void ClearDomainEvents()
  {
    _domainEvents.Clear();
  }
}