using CoreBanking.Core.ValueObjects;
namespace CoreBanking.Core.Events;

public record InsufficientFundEvent : DomainEvent
{
  public AccountNumber AccountNumber { get; }
  public Money RequestedAmount { get; }
  public Money CurrentBalance { get; }
  public string Operation { get; }

  public InsufficientFundEvent(AccountNumber accountNumber, Money requestedAmount, Money currentBalance, string operation)
  {
    AccountNumber = accountNumber;
    RequestedAmount = requestedAmount;
    CurrentBalance = currentBalance;
    Operation = operation;
  }
}