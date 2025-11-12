namespace CoreBanking.Infrastructure.ServiceBus;

public class ServiceBusConfiguration
{
  public string ConnectionString { get; set; } = string.Empty;

  // Topics for domain events
  public string CustomerTopicName { get; set; } = "customer-events";
  public string AccountTopicName { get; set; } = "account-events";
  public string TransactionTopicName { get; set; } = "transaction-events";

  // Queues for commands
  public string AccountCommandQueue { get; set; } = "account-commands";
  public string TransactionCommandQueue { get; set; } = "transaction-commands";

  // Subscriptions for topics
  public Dictionary<string, string[]> TopicSubscriptions { get; set; } = new()
  {
    ["customer-events"] = new[] { "notifications", "reporting", "compliance" },
    ["account-events"] = new[] { "notifications", "analytics", "fraud-detection" },
    ["transaction-events"] = new[] { "notifications", "fraud-detection", "reporting", "analytics" }
  };

  public int MaxRetries { get; set; } = 5;
  public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(5);
  public int PrefetchCount { get; set; } = 10;
  public int MaxConcurrentCalls { get; set; } = 5;
}