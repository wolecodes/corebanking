namespace CoreBanking.Infrastructure.ServiceBus;

public class ServiceBusConfiguration
{
  public string ConnectionString { get; set; } = string.Empty;
  public string CustomerTopicName { get; set; } = "customer-events";
  public string TransactionTopicName { get; set; } = "transaction-events";
  public string AccountQueueName { get; set; } = "account-commands";
  public int MaxRetries { get; set; } = 5;
  public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(5);
}
