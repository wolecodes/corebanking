using System;
using FluentValidation;

namespace CoreBanking.APP.Customers.Commands.CreateCustomer;

public class CreateCustomerValidator : AbstractValidator<CreateCustomerCommand>
{


    public CreateCustomerValidator()
    {
        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Customer Email must be a valid email address");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First Name is required")
            .MinimumLength(3).WithMessage("First Name cannot be less than 3");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last Name  is required")
            .MinimumLength(3).WithMessage("Last Name cannot be less than 3");

    }
}


