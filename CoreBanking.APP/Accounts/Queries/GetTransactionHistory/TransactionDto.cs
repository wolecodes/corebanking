using System;
using CoreBanking.Core.ValueObjects;

namespace CoreBanking.APP.Accounts.Queries.GetTransactionHistory;



public record TransactionDto
{
    public TransactionId? TransactionId { get; init; }
    public string Type { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string Currency { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Reference { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
    public decimal RunningBalance { get; init; }
}
