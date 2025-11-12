using CoreBanking.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
namespace CoreBanking.Infrastructure.ServiceBus;

public class ServiceBusEventPublisher : IEventPublisher
{
  private readonly IServiceBusClientFactory _clientFactory;
  private readonly ServiceBusConfiguration _config;
  private readonly ILogger<ServiceBusEventPublisher> _logger;
  private readonly AsyncRetryPolicy _retryPolicy;

  public ServiceBusEventPublisher(
      IServiceBusClientFactory clientFactory,
      ServiceBusConfiguration config,
      ILogger<ServiceBusEventPublisher> logger)
  {
    _clientFactory = clientFactory;
    _config = config;
    _logger = logger;

    _retryPolicy = Policy
        .Handle<ServiceBusException>(ex => ex.IsTransient)
        .WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            onRetry: (exception, delay, retryCount, context) =>
            {
              _logger.LogWarning(
                          "Retry {RetryCount} for event publishing after {Delay}ms. Error: {Error}",
                          retryCount, delay.TotalMilliseconds, exception.Message);
            });
  }

  public async Task PublishAsync<TEvent>(TEvent domainEvent, CancellationToken cancellationToken = default)
      where TEvent : IDomainEvent
  {
    var topicName = GetTopicNameForEvent(typeof(TEvent));
    var eventType = domainEvent.GetType().Name;

    var message = CreateServiceBusMessage(domainEvent, eventType);

    await _retryPolicy.ExecuteAsync(async () =>
    {
      var sender = _clientFactory.CreateSender(topicName);
      await sender.SendMessageAsync(message, cancellationToken);

      _logger.LogInformation(
                  "Published {EventType} with ID {EventId} to {Topic}",
                  eventType, domainEvent.EventId, topicName);
    });
  }

  public async Task PublishBatchAsync<TEvent>(IEnumerable<TEvent> domainEvents, CancellationToken cancellationToken = default)
      where TEvent : IDomainEvent
  {
    var eventsByTopic = domainEvents.GroupBy(e => GetTopicNameForEvent(e.GetType()));

    foreach (var topicGroup in eventsByTopic)
    {
      var topicName = topicGroup.Key;
      var sender = _clientFactory.CreateSender(topicName);

      using var messageBatch = await sender.CreateMessageBatchAsync(cancellationToken);

      foreach (var domainEvent in topicGroup)
      {
        var eventType = domainEvent.GetType().Name;
        var message = CreateServiceBusMessage(domainEvent, eventType);

        if (!messageBatch.TryAddMessage(message))
        {
          await sender.SendMessagesAsync(messageBatch, cancellationToken);
          _logger.LogInformation("Sent batch of {Count} messages to {Topic}", messageBatch.Count, topicName);

          // Create new batch and add the current message
          using var newBatch = await sender.CreateMessageBatchAsync(cancellationToken);
          newBatch.TryAddMessage(message);
        }
      }

      if (messageBatch.Count > 0)
      {
        await sender.SendMessagesAsync(messageBatch, cancellationToken);
        _logger.LogInformation("Sent final batch of {Count} messages to {Topic}", messageBatch.Count, topicName);
      }
    }
  }

  private ServiceBusMessage CreateServiceBusMessage<TEvent>(TEvent domainEvent, string eventType)
      where TEvent : IDomainEvent
  {
    var eventData = JsonSerializer.Serialize(domainEvent, new JsonSerializerOptions
    {
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
      WriteIndented = false
    });

    var message = new ServiceBusMessage(Encoding.UTF8.GetBytes(eventData))
    {
      ContentType = "application/json",
      MessageId = domainEvent.EventId.ToString(),
      CorrelationId = GetCorrelationId(domainEvent),
      Subject = eventType,
      ApplicationProperties =
                {
                    ["EventType"] = eventType,
                    ["EventId"] = domainEvent.EventId.ToString(),
                    ["OccurredOn"] = domainEvent.OccurredOn,
                    ["Source"] = "CoreBanking",
                    ["Version"] = "1.0",
                    ["AggregateType"] = GetAggregateType(domainEvent),
                    ["AggregateId"] = GetAggregateId(domainEvent)
                }
    };

    // Set time to live based on event type
    message.TimeToLive = eventType switch
    {
      var et when et.Contains("Transaction") => TimeSpan.FromDays(30),
      var et when et.Contains("Account") => TimeSpan.FromDays(90),
      var et when et.Contains("Customer") => TimeSpan.FromDays(365),
      _ => TimeSpan.FromDays(7)
    };

    return message;
  }

  private string GetTopicNameForEvent(Type eventType)
  {
    if (eventType.Name.Contains("Customer")) return _config.CustomerTopicName;
    if (eventType.Name.Contains("Account")) return _config.AccountTopicName;
    if (eventType.Name.Contains("Transaction")) return _config.TransactionTopicName;
    return "general-events";
  }

  private string GetCorrelationId<TEvent>(TEvent domainEvent) where TEvent : IDomainEvent
  {
    // Extract correlation ID from event data or generate one
    return domainEvent.EventId.ToString();
  }

  private string GetAggregateType<TEvent>(TEvent domainEvent) where TEvent : IDomainEvent
  {
    return domainEvent.GetType().Name.Replace("Event", "");
  }

  private string GetAggregateId<TEvent>(TEvent domainEvent) where TEvent : IDomainEvent
  {
    // This would need to be implemented based on your domain event structure
    // For now, return a placeholder
    return "unknown";
  }
}