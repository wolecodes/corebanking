using CoreBanking.Core.Enums;
using CoreBanking.Core.ValueObjects;


namespace CoreBanking.Core.Events
{


    public record AccountCreatedEvent : DomainEvent 
    {
        public AccountId AccountId { get; }
        public AccountNumber AccountNumber { get; }
        public CustomerId CustomerId { get; }
        public AccountType AccountType { get; }
        public Money InitialDeposit { get; }

        public AccountCreatedEvent(AccountId accountId, AccountNumber accountNumber, CustomerId customerId, AccountType accountType, Money initialDeposit)
        {
            AccountId = accountId;
            AccountNumber = accountNumber;
            CustomerId = customerId;
            AccountType = accountType;
            InitialDeposit = initialDeposit;
        }
    }

}