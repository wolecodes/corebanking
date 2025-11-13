namespace CoreBanking.Core.Interfaces;

public interface IEventPublisher
{
  Task PublishAsync<TEvent>(TEvent domainEvent, CancellationToken cancellationToken = default) where TEvent : IDomainEvent;
  Task PublishAsync(string topicName, string eventType, string eventData, CancellationToken cancellationToken = default);
  Task PublishBatchAsync<TEvent>(IEnumerable<TEvent> domainEvents, CancellationToken cancellationToken = default) where TEvent : IDomainEvent;
}