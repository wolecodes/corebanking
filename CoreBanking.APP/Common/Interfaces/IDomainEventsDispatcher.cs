namespace CoreBanking.APP.Common.Interfaces;

public interface IDomainEventDispatcher
{
  Task DispatchDomainEventsAsync(CancellationToken cancellationToken = default);
}