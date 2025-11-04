using System;
using CoreBanking.Core.Entities;

namespace CoreBanking.Core.Interfaces;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(CustomerId customerId);
    Task<IEnumerable<Customer>> GetAllAsync();

    Task AddAsync(Customer customer);
    Task UpdateAsync(Customer customer);
    // Task<bool> ExistsAsync(Guid customerId);
}
