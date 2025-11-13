using Azure.Messaging.ServiceBus;
using CoreBanking.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace CoreBanking.Infrastructure.ServiceBus
{
  public class MockServiceBusSender : IBankingServiceBusSender
  {
    private readonly ILogger<MockServiceBusSender> _logger;

    public MockServiceBusSender(ILogger<MockServiceBusSender> logger) => _logger = logger;

    public Task SendMessageAsync(string queueOrTopicName, string message, CancellationToken cancellationToken = default)
    {
      _logger.LogInformation("MOCK: Would send message to {Destination}: {Message}", queueOrTopicName, message);
      return Task.CompletedTask;
    }

    public Task SendMessageAsync(string queueOrTopicName, byte[] messageBody, IDictionary<string, object> properties = null, CancellationToken cancellationToken = default)
    {
      _logger.LogInformation("MOCK: Would send binary message to {Destination} with {PropertiesCount} properties",
          queueOrTopicName, properties?.Count ?? 0);
      return Task.CompletedTask;
    }

    public Task ScheduleMessageAsync(string queueOrTopicName, string message, DateTimeOffset scheduledEnqueueTime, CancellationToken cancellationToken = default)
    {
      _logger.LogInformation("MOCK: Would schedule message for {ScheduledTime} to {Destination}",
          scheduledEnqueueTime, queueOrTopicName);
      return Task.CompletedTask;
    }

    public Task SendMessageAsync(string queueOrTopicName, string messageBody, IDictionary<string, object> properties = null, CancellationToken cancellationToken = default)
    {
      throw new NotImplementedException();
    }

    public ValueTask DisposeAsync()
    {
      throw new NotImplementedException();
    }

    public Task SendMessageAsync(string queueOrTopicName, ServiceBusMessage message, CancellationToken cancellationToken = default)
    {
      throw new NotImplementedException();
    }
  }
}
