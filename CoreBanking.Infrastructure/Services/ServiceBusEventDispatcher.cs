using CoreBanking.APP.Common.Interfaces;
using CoreBanking.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.Json;
namespace CoreBanking.Infrastructure.Services;


public class ServiceBusEventDispatcher : IDomainEventDispatcher
{
  private readonly IEventPublisher _eventPublisher;
  private readonly ILogger<ServiceBusEventDispatcher> _logger;
  private readonly List<IDomainEvent> _publishedEvents = new();

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
      _publishedEvents.Add(domainEvent);

      _logger.LogInformation(
          "Successfully dispatched domain event {EventType} with ID {EventId}",
          domainEvent.GetType().Name, domainEvent.EventId);
    }
    catch (Exception ex)
    {
      _logger.LogError(
          ex,
          "Failed to dispatch domain event {EventType} with ID {EventId}",
          domainEvent.GetType().Name, domainEvent.EventId);

      // Store failed events for later retry or analysis
      await StoreFailedEventAsync(domainEvent, ex);
      throw;
    }
  }

  public async Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
  {
    var eventsList = domainEvents.ToList();

    if (eventsList.Count > 10)
    {
      // Use batch publishing for large numbers of events
      await _eventPublisher.PublishBatchAsync(eventsList, cancellationToken);
    }
    else
    {
      var dispatchTasks = eventsList.Select(domainEvent => DispatchAsync(domainEvent, cancellationToken));
      await Task.WhenAll(dispatchTasks);
    }

    _logger.LogInformation("Dispatched {EventCount} domain events", eventsList.Count);
  }

  public IReadOnlyList<IDomainEvent> GetPublishedEvents() => _publishedEvents.AsReadOnly();
  public void ClearPublishedEvents() => _publishedEvents.Clear();

  public async Task DispatchDomainEventsAsync(CancellationToken cancellationToken = default)
  {
    // Implement domain event dispatching logic here
    // This method should collect and dispatch all pending domain events
    await Task.CompletedTask;
  }

  private async Task StoreFailedEventAsync(IDomainEvent domainEvent, Exception exception)
  {
    // In a real implementation, you might store this in a database for later analysis/retry
    var failedEventInfo = new
    {
      EventId = domainEvent.EventId,
      EventType = domainEvent.GetType().Name,
      OccurredOn = domainEvent.OccurredOn,
      Exception = exception.Message,
      StackTrace = exception.StackTrace,
      StoredAt = DateTime.UtcNow
    };

    _logger.LogError("Stored failed event: {FailedEventInfo}", JsonSerializer.Serialize(failedEventInfo));

    // TODO: Implement persistent storage for failed events
    await Task.CompletedTask;
  }
}