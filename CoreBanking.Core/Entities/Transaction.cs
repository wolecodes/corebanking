using CoreBanking.Core.Enums;
using CoreBanking.Core.ValueObjects;

namespace CoreBanking.Core.Entities;

public class Transaction
{
    public TransactionId TransactionId { get; private set; }
    public AccountId AccountId { get; private set; }
    public TransactionType Type { get; private set; }
    public Money Amount { get; private set; }
    public string Description { get; private set; }
    public DateTime Timestamp { get; private set; }
    public string Reference { get; private set; }
    public Account Account { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public string? DeletedBy { get; private set; }
    public bool IsArchived { get; private set; } = false;


    private Transaction() { }//Ef Core
    public Transaction(AccountId accountId, Account account, TransactionType type, Money amount, string description, string reference = "")
    {
        TransactionId = TransactionId.Create();
        AccountId = accountId; Account = account;
        Type = type;
        Amount = amount;
        Description = description ?? throw new ArgumentException(nameof(description));
        Timestamp = DateTime.UtcNow;
        Reference = string.IsNullOrEmpty(reference) ? GenerateReference() : reference;
    }
    public static Transaction CreateInterestCredit(AccountId accountId, decimal interestAmount, string description)
    {
        if (interestAmount <= 0)
            throw new ArgumentException("Interest amount must be positive.", nameof(interestAmount));
        var amount = new Money(interestAmount);
        return new Transaction(accountId, null!, TransactionType.Interest, amount, description);
    }
    private string GenerateReference()
    {
        return $"{Timestamp:yyyMMddHHmmss}--{TransactionId.ToString().Substring(0, 8)}";
    }
    public void MarkAsArchived()
    {
        IsArchived = true;
    }
}
