using CoreBanking.Core.Entities;
using CoreBanking.Core.ValueObjects;

namespace CoreBanking.Core.Interfaces
{
    public interface IAccountRepository
    {

        Task<List<Account>> GetAllAsync();
        Task<Account?> GetByAccountNumberAsync(AccountNumber accountNumber);
        Task<IEnumerable<Account>> GetByCustomerIdAsync(CustomerId customerId);
        Task AddAsync(Account account);
        Task UpdateAsync(Account account);
        Task<bool> AccountNumberExistsAsync(AccountNumber accountNumber);

        Task<Account> GetByIdAsync(AccountId accountId, CancellationToken cancellationToken = default);
        Task<Account> GetByAccountNumberAsync(AccountNumber accountNumber, CancellationToken cancellationToken = default);
        Task<List<Account>> GetAccountsByCustomerIdAsync(CustomerId customerId, CancellationToken cancellationToken = default);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);

        // NEW METHODS FOR BACKGROUND JOBS
        Task<List<Account>> GetInactiveAccountsSinceAsync(DateTime sinceDate, CancellationToken cancellationToken = default);
        Task<List<Account>> GetInterestBearingAccountsAsync(CancellationToken cancellationToken = default);
        Task<List<Account>> GetActiveAccountsAsync(CancellationToken cancellationToken = default);
        Task<List<Account>> GetAccountsByStatusAsync(string status, CancellationToken cancellationToken = default);
        Task<List<Account>> GetAccountsWithLowBalanceAsync(decimal minimumBalance, CancellationToken cancellationToken = default);
    }
}
