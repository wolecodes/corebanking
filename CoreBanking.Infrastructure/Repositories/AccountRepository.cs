using CoreBanking.Core.Entities;
using CoreBanking.Core.Exceptions;
using CoreBanking.Core.Interfaces;
using CoreBanking.Core.ValueObjects;
using CoreBanking.Infrastructure.Data;
using CoreBanking.Core.Enums;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace CoreBanking.Infrastructure.Repositories
{
    public class AccountRepository : IAccountRepository
    {
        private readonly BankingDbContext _context;

        public AccountRepository(BankingDbContext context)
        {
            _context = context;
        }

        public async Task<Account?> GetByIdAsync(AccountId accountId, CancellationToken cancellationToken = default)
        {
            return await _context.Accounts
                .Include(a => a.Customer)
                .Include(a => a.Transactions)
                .FirstOrDefaultAsync(a => a.AccountId == accountId, cancellationToken);
        }

        public async Task<List<Account>> GetAllAsync()
        {
            return await _context.Accounts
            .Include(a => a.Customer)
            .Include(a => a.Transactions)
            .ToListAsync();
        }

        public async Task<Account?> GetByAccountNumberAsync(AccountNumber accountNumber)
        {
            return await _context.Accounts
                .Include(a => a.Customer)  // Include Customer for queries
                .Include(a => a.Transactions)

                .FirstOrDefaultAsync(a => a.AccountNumber == accountNumber);
        }

        public async Task<IEnumerable<Account>> GetByCustomerIdAsync(CustomerId customerId)
        {
            return await _context.Accounts
                .Where(a => a.CustomerId == customerId)
                .Include(a => a.Transactions)
                .ToListAsync();
        }

        public async Task AddAsync(Account account)
        {
            await _context.Accounts.AddAsync(account);
        }

        public async Task UpdateAsync(Account account)
        {
            _context.Accounts.Update(account);
            await Task.CompletedTask;
        }

        public async Task UpdateAccountBalanceAsync(AccountId accountId, Money newBalance)
        {
            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.AccountId == accountId);

            if (account == null)
                throw new InvalidOperationException("Account not found.");

            // Replace the value object
            account.UpdateBalance(newBalance);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new ConcurrencyException("Account was modified by another user. Please refresh and try again.");
            }
        }

        public async Task<bool> AccountNumberExistsAsync(AccountNumber accountNumber)
        {
            return await _context.Accounts
                .AnyAsync(a => a.AccountNumber == accountNumber);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<List<Account>> GetInactiveAccountsSinceAsync(DateTime sinceDate, CancellationToken cancellationToken = default)
        {
            return await _context.Accounts
                .Include(a => a.Customer)
                .Where(a => a.LastActivityDate < sinceDate &&
                        a.Status == "Active" && // Only active accounts
                        a.Balance.Amount == 0)  // Only zero balance accounts
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Account>> GetInterestBearingAccountsAsync(CancellationToken cancellationToken = default)
        {
            var interestBearingTypes = new List<AccountType> { AccountType.Savings, AccountType.FixedDepost };

            return await _context.Accounts
                .Include(a => a.Customer)
                .Where(a => interestBearingTypes.Contains(a.AccountType) &&
                        a.Status == "Active" &&
                        a.IsInterestBearing)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Account>> GetActiveAccountsAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Accounts
                .Include(a => a.Customer)
                .Where(a => a.Status == "Active")
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Account>> GetAccountsByStatusAsync(string status, CancellationToken cancellationToken = default)
        {
            return await _context.Accounts
                .Include(a => a.Customer)
                .Where(a => a.Status == status)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Account>> GetAccountsWithLowBalanceAsync(decimal minimumBalance, CancellationToken cancellationToken = default)
        {
            return await _context.Accounts
                .Include(a => a.Customer)
                .Where(a => a.Balance.Amount < minimumBalance &&
                        a.Status == "Active")
                .ToListAsync(cancellationToken);
        }

        // Other existing methods...
        public async Task<Account> GetByAccountNumberAsync(AccountNumber accountNumber, CancellationToken cancellationToken = default)
        {
            return await _context.Accounts
                .Include(a => a.Customer)
                .FirstOrDefaultAsync(a => a.AccountNumber == accountNumber, cancellationToken);
        }

        public async Task<List<Account>> GetAccountsByCustomerIdAsync(CustomerId customerId, CancellationToken cancellationToken = default)
        {
            return await _context.Accounts
                .Include(a => a.Customer)
                .Where(a => a.Customer.CustomerId == customerId)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(Account entity, CancellationToken cancellationToken = default)
        {
            await _context.Accounts.AddAsync(entity, cancellationToken);
        }

        public async Task UpdateAsync(Account entity, CancellationToken cancellationToken = default)
        {
            _context.Accounts.Update(entity);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(Account entity, CancellationToken cancellationToken = default)
        {
            _context.Accounts.Remove(entity);
            await Task.CompletedTask;
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
