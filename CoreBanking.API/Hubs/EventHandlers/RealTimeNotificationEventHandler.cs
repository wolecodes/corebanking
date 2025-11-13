using MediatR;
using Microsoft.AspNetCore.SignalR;
using CoreBanking.Core.Events;
using CoreBanking.API.Hubs.Interfaces;
using CoreBanking.API.Hubs.Models;

namespace CoreBanking.API.Hubs.EventHandlers;

public class RealTimeNotificationEventHandler : INotificationHandler<MoneyTransferedEvent>
{
  private readonly ILogger<RealTimeNotificationEventHandler> _logger;
  private readonly IHubContext<EnhancedNotificationHub, IBankingClient> _hubContext;

  public RealTimeNotificationEventHandler(
      ILogger<RealTimeNotificationEventHandler> logger,
      IHubContext<EnhancedNotificationHub, IBankingClient> hubContext)
  {
    _logger = logger;
    _hubContext = hubContext;
  }

  public async Task Handle(MoneyTransferedEvent notification, CancellationToken cancellationToken)
  {
    _logger.LogInformation(
        "Processing real-time notification for transfer {TransactionId}",
        notification.TransactionId);

    try
    {
      // Notify source account
      var sourceNotification = new TransactionNotification
      {
        TransactionId = notification.TransactionId.ToString(),
        AccountNumber = notification.SourceAccountNumber.ToString(),
        Amount = -notification.Amount.Amount, // Negative for debit
        Type = "Debit",
        Description = $"Transfer to {notification.DestinationAccountNumber}",
        Timestamp = notification.TransferDate,
        RunningBalance = 0 // Would need to fetch current balance
      };

      await _hubContext.Clients.Group($"account-{notification.SourceAccountNumber}")
          .ReceiveTransactionNotification(sourceNotification);

      // Notify destination account
      var destNotification = new TransactionNotification
      {
        TransactionId = notification.TransactionId.ToString(),
        AccountNumber = notification.DestinationAccountNumber.ToString(),
        Amount = notification.Amount.Amount, // Positive for credit
        Type = "Credit",
        Description = $"Transfer from {notification.SourceAccountNumber}",
        Timestamp = notification.TransferDate,
        RunningBalance = 0 // Would need to fetch current balance
      };

      await _hubContext.Clients.Group($"account-{notification.DestinationAccountNumber}")
          .ReceiveTransactionNotification(destNotification);

      _logger.LogInformation(
          "Sent real-time notifications for transfer {TransactionId}",
          notification.TransactionId);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex,
          "Failed to send real-time notifications for transfer {TransactionId}",
          notification.TransactionId);
    }
  }
}