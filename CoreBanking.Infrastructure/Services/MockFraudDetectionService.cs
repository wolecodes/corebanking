using CoreBanking.Core.Events;
using CoreBanking.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace CoreBanking.Infrastructure.Services
{
  public class MockFraudDetectionService : IFraudDetectionService
  {
    private readonly ILogger<MockFraudDetectionService> _logger;

    public MockFraudDetectionService(ILogger<MockFraudDetectionService> logger)
    {
      _logger = logger;
    }

    public async Task<FraudDetectionResult> CheckTransactionAsync(MoneyTransferedEvent transactionEvent, CancellationToken cancellationToken)
    {
      _logger.LogInformation("Checking transaction {TransactionId} for fraud", transactionEvent.TransactionId);

      // Simulate processing time
      await Task.Delay(100, cancellationToken);

      // Simple fraud detection rules
      var result = new FraudDetectionResult();

      // Rule 1: Large amount transfers
      if (transactionEvent.Amount.Amount > 10000)
      {
        result.IsSuspicious = true;
        result.RiskScore = 75;
        result.Reason = $"Large transfer amount: {transactionEvent.Amount:C}";
        _logger.LogWarning("Large amount detected: {Amount}", transactionEvent.Amount);
      }

      // Rule 2: Rapid repeated transfers (you'd need more context for this)
      // Rule 3: Unusual time of day, etc.

      // If no fraud detected
      if (!result.IsSuspicious)
      {
        result.RiskScore = 5; // Low risk
        result.Reason = "Transaction appears legitimate";
      }

      _logger.LogInformation("Fraud check completed for {TransactionId}. Suspicious: {IsSuspicious}",
          transactionEvent.TransactionId, result.IsSuspicious);

      return result;
    }
  }
}