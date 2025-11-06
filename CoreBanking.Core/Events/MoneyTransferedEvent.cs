using CoreBanking.Core.ValueObjects;


namespace CoreBanking.Core.Events
{

    public record MoneyTransferedEvent : DomainEvent
    {
        public TransactionId TransactionId { get; }
        public AccountNumber SourceAccount { get; }
        public AccountNumber DestinationAccount { get; }
        public Money Amount { get; }
        public string Reference { get; }
        public DateTime TransferDate { get; }

        public MoneyTransferedEvent(TransactionId transactionId, AccountNumber sourceAccount, AccountNumber destinationAccount, Money amount, string reference)
        {
            TransactionId = transactionId;
            SourceAccount = sourceAccount;
            DestinationAccount = destinationAccount;
            Amount = amount;
            Reference = reference;
            TransferDate = DateTime.UtcNow;
        }
    }


}