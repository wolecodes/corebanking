using CoreBanking.APP.Common.Interfaces;
using MediatR;
using CoreBanking.Core.ValueObjects;
using CoreBanking.APP.Common.Models;
using CoreBanking.Core.Interfaces;


namespace CoreBanking.APP.Customers.Queries.GetCustomerDetails;

public record GetCustomerDetailsQuery : IQuery<CustomerDetailsDto>
{
    public Guid CustomerId { get; init; }
}





public class GetCustomersDetailsQueryHandler : IRequestHandler<GetCustomerDetailsQuery, Result<CustomerDetailsDto>>
{
    private readonly ICustomerRepository _customerRepository;

    public GetCustomersDetailsQueryHandler(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }


    public async Task<Result<CustomerDetailsDto>> Handle(GetCustomerDetailsQuery request, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdAsync(CustomerId.Create(request.CustomerId));

        if (customer == null)
            return Result<CustomerDetailsDto>.Failure("Account not found");

        var dto = new CustomerDetailsDto
        {
            CustomerId = customer.CustomerId,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            Email = customer.Email,
            Phone = customer.PhoneNumber,
            DateRegistered = customer.DateCreated,
            IsActive = customer.IsActive
        };

        return Result<CustomerDetailsDto>.Success(dto);
    }
}

