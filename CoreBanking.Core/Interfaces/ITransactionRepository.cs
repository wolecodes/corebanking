

using CoreBanking.Core.Entities;
using CoreBanking.Core.ValueObjects;

namespace CoreBanking.Core.Interfaces
{
    public interface ITransactionRepository
    {
        Task<Transaction?> GetByIdAsync(TransactionId transactionId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Transaction>> GetByAccountIdAsync(AccountId accountId, CancellationToken cancellationToken = default);

        Task<IEnumerable<Transaction>> GetByAccountIdAndDateRangeAsync(AccountId accountId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
        Task AddRangeAsync(IEnumerable<Transaction> entities, CancellationToken cancellationToken = default);
        Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default);

        Task UpdateAsync(Transaction transaction, CancellationToken cancellationToken = default);

        Task<List<Transaction>> GetTransactionsBeforeAsync(DateTime cutoffDate, CancellationToken cancellationToken = default);
        Task<List<Transaction>> GetRecentTransactionsByAccountAsync(AccountId accountId, DateTime sinceDate, CancellationToken cancellationToken = default);
        Task<List<Transaction>> GetTransactionsByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
        Task<List<Transaction>> GetTransactionsByAccountAndDateRangeAsync(AccountId accountId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
        Task<decimal> GetTotalTransactionsAmountByDateAsync(DateTime date, CancellationToken cancellationToken = default);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
