using CoreBanking.Core.Entities;
using CoreBanking.Core.Interfaces;
using CoreBanking.Core.ValueObjects;
using CoreBanking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CoreBanking.Infrastructure.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly BankingDbContext _context;

    public TransactionRepository(BankingDbContext context)
    {
        _context = context;
    }

    public async Task<Transaction?> GetByIdAsync(TransactionId transactionId, CancellationToken cancellationToken = default)
    {
        return await _context.Transactions.FirstOrDefaultAsync(t => t.TransactionId == transactionId, cancellationToken);
        // return await _context.Transactions.FirstOrDefaultAsync(t => t.TransactionId == transactionId, cancellationToken);
    }

    public async Task<IEnumerable<Transaction>> GetByAccountIdAsync(AccountId accountId, CancellationToken cancellationToken = default)
    {
        return await _context.Transactions
            .Where(t => t.AccountId == accountId)
            .OrderByDescending(t => t.Timestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Transaction>> GetByAccountIdAndDateRangeAsync(AccountId accountId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _context.Transactions
            .Where(t => t.AccountId == accountId &&
            t.Timestamp >= startDate &&
            t.Timestamp <= endDate)
            .OrderBy(t => t.Timestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        await _context.Transactions.AddAsync(transaction, cancellationToken);
    }

    public async Task UpdateAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        _context.Transactions.Update(transaction);
        await Task.CompletedTask;
    }

    public async Task<List<Transaction>> GetTransactionsBeforeAsync(DateTime cutoffDate, CancellationToken cancellationToken = default)
    {
        return await _context.Transactions
            .Include(t => t.Account)
            .ThenInclude(a => a.Customer)
            .Where(t => t.Timestamp < cutoffDate &&
                    !t.IsArchived) // Only non-archived transactions
            .OrderBy(t => t.Timestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Transaction>> GetRecentTransactionsByAccountAsync(AccountId accountId, DateTime sinceDate, CancellationToken cancellationToken = default)
    {
        return await _context.Transactions
            .Include(t => t.Account)
            .ThenInclude(a => a.Customer)
            .Where(t => t.Account.AccountId == accountId &&
                    t.Timestamp >= sinceDate &&
                    !t.IsArchived)
            .OrderByDescending(t => t.Timestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Transaction>> GetTransactionsByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _context.Transactions
            .Include(t => t.Account)
            .ThenInclude(a => a.Customer)
            .Where(t => t.Timestamp >= startDate &&
                    t.Timestamp <= endDate &&
                    !t.IsArchived)
            .OrderBy(t => t.Timestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Transaction>> GetTransactionsByAccountAndDateRangeAsync(AccountId accountId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _context.Transactions
            .Include(t => t.Account)
            .ThenInclude(a => a.Customer)
            .Where(t => t.Account.AccountId == accountId &&
                    t.Timestamp >= startDate &&
                    t.Timestamp <= endDate &&
                    !t.IsArchived)
            .OrderBy(t => t.Timestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task<decimal> GetTotalTransactionsAmountByDateAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        var startOfDay = date.Date;
        var endOfDay = date.Date.AddDays(1).AddTicks(-1);

        return await _context.Transactions
            .Where(t => t.Timestamp >= startOfDay &&
                    t.Timestamp <= endOfDay &&
                    t.Type == CoreBanking.Core.Enums.TransactionType.Deposit) // Or whatever type you need
            .SumAsync(t => t.Amount.Amount, cancellationToken);
    }

    // Other existing methods...
    public async Task<List<Transaction>> GetTransactionsByAccountIdAsync(AccountId accountId, CancellationToken cancellationToken = default)
    {
        return await _context.Transactions
            .Include(t => t.Account)
            .ThenInclude(a => a.Customer)
            .Where(t => t.Account.AccountId == accountId)
            .OrderByDescending(t => t.Timestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<Transaction> entities, CancellationToken cancellationToken = default)
    {
        await _context.Transactions.AddRangeAsync(entities, cancellationToken);
    }

    public async Task DeleteAsync(Transaction entity, CancellationToken cancellationToken = default)
    {
        _context.Transactions.Remove(entity);
        await Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
