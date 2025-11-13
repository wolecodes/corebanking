using CoreBanking.Core.Events;

namespace CoreBanking.Core.Interfaces;

public interface IFraudDetectionService
{
  Task<FraudDetectionResult> CheckTransactionAsync(MoneyTransferedEvent transactionEvent, CancellationToken cancellationToken);
}

public class FraudDetectionResult
{
  public bool IsSuspicious { get; set; }
  public decimal RiskScore { get; set; }
  public string Reason { get; set; } = string.Empty;
}