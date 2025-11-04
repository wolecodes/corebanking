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
    public Account? Account { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public string? DeletedBy { get; private set; }


    private Transaction() { }
    public Transaction(AccountId accountId, TransactionType type, Money amount, string description, string reference = "")
    {
        TransactionId = TransactionId.Create();
        AccountId = accountId;
        Type = type;
        Amount = amount;
        Description = description ?? throw new ArgumentException(nameof(description));
        Timestamp = DateTime.UtcNow;
        Reference = string.IsNullOrEmpty(reference) ? GenerateReference() : reference;
    }
    private string GenerateReference()
    {
        return $"{Timestamp:yyyMMddHHmmss}--{TransactionId.ToString().Substring(0, 8)}";
    }
}
