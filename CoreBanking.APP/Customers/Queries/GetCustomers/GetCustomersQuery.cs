using CoreBanking.APP.Common.Interfaces;
using CoreBanking.APP.Common.Models;
using CoreBanking.Core.Interfaces;
using MediatR;

namespace CoreBanking.APP.Customers.Queries.GetCustomers;

public record GetCustomersQuery : IQuery<List<CustomerDto>>;

public class GetCustomersQueryHandler : IRequestHandler<GetCustomersQuery, Result<List<CustomerDto>>>
{
    private readonly ICustomerRepository _customerRepository;

    public GetCustomersQueryHandler(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<Result<List<CustomerDto>>> Handle(GetCustomersQuery request, CancellationToken cancellationToken)
    {
        var customers = await _customerRepository.GetAllAsync();

        var customerDtos = customers.Select(customer => new CustomerDto
        {
            CustomerId = customer.CustomerId,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            Email = customer.Email,
            Phone = customer.PhoneNumber,
            DateRegistered = customer.DateCreated,
            IsActive = customer.IsActive
        }).ToList();

        return Result<List<CustomerDto>>.Success(customerDtos);
    }
}
