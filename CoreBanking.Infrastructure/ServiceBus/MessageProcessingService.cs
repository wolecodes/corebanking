namespace CoreBanking.Infrastructure.ServiceBus;

public class MessageProcessingService : BackgroundService
{
  private readonly IServiceProvider _serviceProvider;
  private readonly ILogger<MessageProcessingService> _logger;
  private readonly List<IAsyncDisposable> _processors = new();

  public MessageProcessingService(IServiceProvider serviceProvider, ILogger<MessageProcessingService> logger)
  {
    _serviceProvider = serviceProvider;
    _logger = logger;
  }

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    _logger.LogInformation("Starting message processing service");

    // Start all message handlers
    await StartCustomerEventHandlerAsync(stoppingToken);
    await StartTransactionEventHandlerAsync(stoppingToken);
    await StartAccountEventHandlerAsync(stoppingToken);

    _logger.LogInformation("All message handlers started");

    // Keep the service running until stopped
    while (!stoppingToken.IsCancellationRequested)
    {
      await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
    }

    _logger.LogInformation("Stopping message processing service");
  }

  private async Task StartCustomerEventHandlerAsync(CancellationToken stoppingToken)
  {
    using var scope = _serviceProvider.CreateScope();
    var handler = scope.ServiceProvider.GetRequiredService<CustomerEventHandler>();
    _processors.Add(handler);
    await handler.StartProcessingAsync(stoppingToken);
  }

  private async Task StartTransactionEventHandlerAsync(CancellationToken stoppingToken)
  {
    using var scope = _serviceProvider.CreateScope();
    var handler = scope.ServiceProvider.GetRequiredService<TransactionEventHandler>();
    _processors.Add(handler);
    await handler.StartProcessingAsync(stoppingToken);
  }

  private async Task StartAccountEventHandlerAsync(CancellationToken stoppingToken)
  {
    using var scope = _serviceProvider.CreateScope();
    var handler = scope.ServiceProvider.GetRequiredService<AccountEventHandler>();
    _processors.Add(handler);
    await handler.StartProcessingAsync(stoppingToken);
  }

  public override async Task StopAsync(CancellationToken cancellationToken)
  {
    _logger.LogInformation("Stopping all message processors");

    foreach (var processor in _processors)
    {
      try
      {
        await processor.DisposeAsync();
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error disposing message processor");
      }
    }

    await base.StopAsync(cancellationToken);
  }
}