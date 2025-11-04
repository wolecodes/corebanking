namespace CoreBanking.APP.Common.Interfaces;

public interface IDomainEventDispatcher
{
  Task DispatchDomainEvents(CancellationToken cancellationToken = default);
}