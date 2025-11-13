using Azure.Messaging.ServiceBus;

namespace CoreBanking.Infrastructure.ServiceBus;

public interface IBankingServiceBusSender : IAsyncDisposable
{
  Task SendMessageAsync(string queueOrTopicName, string message, CancellationToken cancellationToken = default);
  Task SendMessageAsync(string queueOrTopicName, byte[] messageBody, IDictionary<string, object> properties = null, CancellationToken cancellationToken = default);
  Task SendMessageAsync(string queueOrTopicName, string messageBody, IDictionary<string, object> properties = null, CancellationToken cancellationToken = default);
  Task ScheduleMessageAsync(string queueOrTopicName, string message, DateTimeOffset scheduledEnqueueTime, CancellationToken cancellationToken = default);
  Task SendMessageAsync(string queueOrTopicName, ServiceBusMessage message, CancellationToken cancellationToken = default);
}