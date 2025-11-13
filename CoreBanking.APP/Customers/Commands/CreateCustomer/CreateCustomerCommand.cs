using CoreBanking.APP.Common.Interfaces;
using CoreBanking.Core.ValueObjects;
namespace CoreBanking.APP.Customers.Commands.CreateCustomer;

public record CreateCustomerCommand : ICommand<CustomerId>
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public DateTime DateOfBirth { get; init; }
    public string BVN { get; init; } = string.Empty;

}


