using AutoMapper;
using CoreBanking.API.Models.Requests;
using CoreBanking.APP.Accounts.Commands.CreateAccount;
using CoreBanking.APP.Customers.Commands.CreateCustomer;
using CoreBanking.Core.ValueObjects;


namespace CoreBanking.API.Mappings;

public class RequestToCommandProfile : Profile
{
    public RequestToCommandProfile()
    {
        // Map API Request DTOs to Application Commands

        // CreateAccountRequest -> CreateAccountCommand
        CreateMap<CreateAccountRequest, CreateAccountCommand>()
            .ForMember(dest => dest.CustomerId,
                opt => opt.MapFrom(src => CustomerId.Create(src.CustomerId)))
            .ForMember(dest => dest.AccountType,
                opt => opt.MapFrom(src => src.AccountType ?? string.Empty));

        // CreateCustomerRequest -> CreateCustomerCommand
        // All properties match exactly (string to string, DateTime to DateTime)
        // AutoMapper can map automatically, but explicit mapping is better practice
        CreateMap<CreateCustomerRequest, CreateCustomerCommand>();
    }
}

