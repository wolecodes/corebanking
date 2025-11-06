using CoreBanking.APP.Common.Interfaces;
using Microsoft.Extensions.Logging;
using CoreBanking.Infrastructure.Data;
using CoreBanking.Core.Interfaces;

using MediatR;

namespace CoreBanking.Infrastructure.Services
{

  public class DomainEventDispatcher : IDomainEventDispatcher
  {
    private readonly BankingDbContext _context;
    private readonly IPublisher _publisher;
    private readonly ILogger<DomainEventDispatcher> _logger;

    public DomainEventDispatcher(
        BankingDbContext context,
        IPublisher publisher,
        ILogger<DomainEventDispatcher> logger)
    {
      _context = context;
      _publisher = publisher;
      _logger = logger;
    }

    public async Task DispatchDomainEventsAsync(CancellationToken cancellationToken = default)
    {
      var domainEntities = _context.ChangeTracker
          .Entries<IAggregateRoot>()
          .Where(x => x.Entity.DomainEvents.Any())
          .ToList();

      var domainEvents = domainEntities
          .SelectMany(x => x.Entity.DomainEvents)
          .ToList();

      foreach (var domainEvent in domainEvents)
      {
        _logger.LogInformation("Dispatching domain event: {EventType}", domainEvent.GetType().Name);
        await _publisher.Publish(domainEvent, cancellationToken);
      }

      domainEntities.ForEach(entity => entity.Entity.ClearDomainEvents());
    }
  }
}