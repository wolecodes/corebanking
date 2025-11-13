using CoreBanking.Core.Interfaces;
using CoreBanking.Core.ValueObjects;
using MediatR;

namespace CoreBanking.Core.Events
{
  public class CustomerCreatedEvent : IDomainEvent, INotification
  {
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public CustomerId CustomerId { get; }
    public string FirstName { get; }
    public string LastName { get; }
    public string Email { get; }
    public string PhoneNumber { get; }
    public int CreditScore { get; }

    public string EventType => throw new NotImplementedException();

    public CustomerCreatedEvent(CustomerId customerId, string firstName, string lastName, string email, string phoneNumber, int creditScore)
    {
      CustomerId = customerId;
      FirstName = firstName;
      LastName = lastName;
      Email = email;
      PhoneNumber = phoneNumber;
      CreditScore = creditScore;
    }
  }
}
