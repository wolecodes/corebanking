

namespace CoreBanking.Core.ValueObjects
{
    public class Money
    {
        public decimal Amount { get; }  // ✅ Renamed from Account to Amount
        public string Currency { get; } = "USD";

        public Money(decimal amount, string currency = "USD")
        {
            if (amount < 0)
                throw new ArgumentException("Money amount cannot be negative");

            Amount = amount;
            Currency = currency;
        }

        public static Money operator +(Money a, Money b)
        {
            if (a.Currency != b.Currency)
                throw new InvalidOperationException("Cannot add different currencies");

            return new Money(a.Amount + b.Amount, a.Currency);
        }

        public static Money operator -(Money a, Money b)
        {
            if (a.Currency != b.Currency)
                throw new InvalidOperationException("Cannot subtract different currencies");

            return new Money(a.Amount - b.Amount, a.Currency);
        }
    }
}
