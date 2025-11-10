using System;
using CoreBanking.Core.Interfaces;

namespace CoreBanking.APP.Common.Interfaces;

public interface IDomainEventDispatcher
{
  Task DispatchDomainEventsAsync(CancellationToken cancellationToken);
  Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);
  Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
}