namespace CoreBanking.Core.Interfaces;

public interface INotificationBroadcaster
{
  Task BroadcastCustomerCreatedAsync(Guid customerId, string customerName, string email, int creditScore);
  Task BroadcastTransactionAsync(Guid transactionId, decimal amount, string transactionType, string fromAccount, string toAccount);
  Task BroadcastFraudAlertAsync(Guid transactionId, string reason, decimal amount);
}