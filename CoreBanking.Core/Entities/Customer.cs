using CoreBanking.Core.ValueObjects;


namespace CoreBanking.Core.Entities;

public class Customer : ISoftDelete
{
    public CustomerId CustomerId { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string Email { get; private set; }
    public string PhoneNumber { get; private set; }
    public DateTime DateCreated { get; private set; }
    public bool IsActive { get; private set; }

    // for softdelete
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public string? DeletedBy { get; private set; }
    public string Address { get; private set; }
    public DateTime DateOfBirth { get; private set; }

    public string BVN { get; private set; }
    public int CreditScore { get; private set; }


    private readonly List<Account> _accounts = new();
    public IReadOnlyCollection<Account> Accounts => _accounts.AsReadOnly();

    public Customer(string firstName, string lastName, string email, string phoneNumber, DateTime dateOfBirth, string bVN, int creditScore)
    {
        CustomerId = CustomerId.Create();
        FirstName = firstName ?? throw new ArgumentException(nameof(firstName));
        LastName = lastName ?? throw new ArgumentException(nameof(lastName));
        Email = email ?? throw new ArgumentException(nameof(email));
        PhoneNumber = phoneNumber ?? throw new ArgumentException(nameof(phoneNumber));
        DateCreated = DateTime.UtcNow;
        IsActive = true;
        DateOfBirth = dateOfBirth;
        CreditScore = creditScore;
        BVN = bVN;
    }

    public void UpdateContactInfo(string email, string phoneNumber)
    {
        if (!IsActive) throw new InvalidOperationException("cannot update inactive customer");
        Email = email;
        PhoneNumber = phoneNumber;
    }

    public void Deactivate()
    {
        if (_accounts.Any(a => a.Balance.Amount > 0))
            throw new InvalidOperationException("Cannot deactivate customer with account balance");

        IsActive = false;
    }

    internal void AddAccount(Account account)
    {
        _accounts.Add(account);
    }

    // for softdelete

    public void SoftDelete(string deletedBy)
    {
        if (Accounts.Any(a => a.Balance.Amount > 0))
            throw new InvalidOperationException("Cannot delete customer with account balance");

        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        DeletedBy = deletedBy;
    }


}

