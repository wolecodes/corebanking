using CoreBanking.APP.Common.Interfaces;
using CoreBanking.Core.Interfaces;
using Microsoft.Extensions.Logging;
namespace CoreBanking.Infrastructure.Services;


public class ServiceBusEventDispatcher : IDomainEventDispatcher
{
  private readonly IEventPublisher _eventPublisher;
  private readonly ILogger<ServiceBusEventDispatcher> _logger;

  public ServiceBusEventDispatcher(IEventPublisher eventPublisher, ILogger<ServiceBusEventDispatcher> logger)
  {
    _eventPublisher = eventPublisher;
    _logger = logger;
  }

  public async Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
  {
    try
    {
      await _eventPublisher.PublishAsync(domainEvent, cancellationToken);

      _logger.LogInformation(
          "Successfully dispatched domain event {EventType} with ID {EventId}",
          domainEvent.GetType().Name, domainEvent);
    }
    catch (Exception ex)
    {
      _logger.LogError(
          ex,
          "Failed to dispatch domain event {EventType} with ID {EventId}",
          domainEvent.GetType().Name, domainEvent.EventId);

      // In production, you might want to store failed events for retry
      throw;
    }
  }

  public async Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
  {
    var dispatchTasks = domainEvents.Select(domainEvent => DispatchAsync(domainEvent, cancellationToken));
    await Task.WhenAll(dispatchTasks);

    _logger.LogInformation("Dispatched {EventCount} domain events", domainEvents.Count());
  }

  public async Task DispatchDomainEventsAsync(CancellationToken cancellationToken = default)
  {
    // Implementation for dispatching domain events
    // This method should be implemented based on your domain event collection strategy
    _logger.LogInformation("DispatchDomainEventsAsync called");
    await Task.CompletedTask;
  }
}