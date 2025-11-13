
using CoreBanking.Core.Entities;
using CoreBanking.Infrastructure.Data;
using CoreBanking.Core.Interfaces;
using CoreBanking.Core.ValueObjects;
using Microsoft.EntityFrameworkCore;


namespace CoreBanking.Infrastructure.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly BankingDbContext _context;

        public CustomerRepository(BankingDbContext context)
        {
            _context = context;
        }

        public async Task<Customer?> GetByIdAsync(CustomerId customerId, CancellationToken cancellationToken = default)
        {
            return await _context.Customers
                .Include(c => c.Accounts)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId, cancellationToken);
        }

        public async Task<IEnumerable<Customer>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Customers
                .Include(c => c.Accounts)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(Customer customer, CancellationToken cancellationToken = default)
        {
            await _context.Customers.AddAsync(customer, cancellationToken);
        }

        public async Task UpdateAsync(Customer customer, CancellationToken cancellationToken = default)
        {
            _context.Customers.Update(customer);
            await Task.CompletedTask;
        }

        public async Task<bool> ExistsAsync(CustomerId customerId, CancellationToken cancellationToken = default)
        {
            return await _context.Customers
                .AnyAsync(c => c.CustomerId == customerId, cancellationToken);
        }
    }
}
