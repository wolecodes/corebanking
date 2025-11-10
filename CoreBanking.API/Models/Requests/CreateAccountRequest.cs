using System;
using CoreBanking.APP.Accounts.Commands.TransferMoney;
using FluentValidation;

namespace CoreBanking.API.Models.Requests;



public record CreateAccountRequest
{
    public Guid CustomerId { get; init; }
    public string? AccountType { get; init; }
    public decimal InitialDeposit { get; init; }
    public string Currency { get; init; } = "NGN";
}

// CoreBanking.API/Models/Requests/TransferMoneyRequest.cs
public record TransferMoneyRequest
{
    public string DestinationAccountNumber { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "NGN";
    public string Reference { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}

// CoreBanking.API/Models/Requests/CreateCustomerRequest.cs
public record CreateCustomerRequest
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public DateTime DateOfBirth { get; init; }
    public string BVN { get; init; } = string.Empty;
}

