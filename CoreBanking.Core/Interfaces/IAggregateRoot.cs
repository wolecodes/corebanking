

namespace CoreBanking.Core.Interfaces
{
  public interface IAggregateRoot
  {
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }
    void ClearDomainEvents();
  }
}