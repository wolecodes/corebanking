using CoreBanking.Core.ValueObjects;

namespace CoreBanking.APP.BackgroundJobs.Results;

public class InterestCalculationResult
{
  // Properties for individual account results
  public bool IsSuccess { get; private set; }
  public string ErrorMessage { get; private set; } = string.Empty;
  public decimal InterestAmount { get; private set; }
  public AccountId AccountId { get; private set; } = AccountId.Create(Guid.Empty);
  public DateTime CalculationDate { get; private set; } = DateTime.UtcNow;

  // Properties for batch operations
  public int SuccessfulCalculations { get; private set; }
  public int FailedCalculations { get; private set; }
  public decimal TotalInterest { get; private set; }
  public List<AccountInterestDetail> AccountDetails { get; private set; } = new();

  // Parameterless constructor for batch operations
  public InterestCalculationResult()
  {
    // Used for batch operations where we'll increment counters
  }

  // Private constructor for individual account results
  private InterestCalculationResult(decimal interestAmount, AccountId accountId, DateTime calculationDate, bool isSuccess, string errorMessage = "")
  {
    IsSuccess = isSuccess;
    InterestAmount = interestAmount;
    AccountId = accountId;
    CalculationDate = calculationDate;
    ErrorMessage = errorMessage;

    // For individual results, set batch properties accordingly
    if (isSuccess)
    {
      SuccessfulCalculations = 1;
      TotalInterest = interestAmount;
    }
    else
    {
      FailedCalculations = 1;
    }
  }

  // KEEP YOUR EXISTING STATIC METHODS (with updated implementation)
  public static InterestCalculationResult Success(decimal interestAmount, AccountId accountId, DateTime calculationDate)
  {
    return new InterestCalculationResult(interestAmount, accountId, calculationDate, true);
  }

  public static InterestCalculationResult Failure(string errorMessage, AccountId accountId, DateTime calculationDate)
  {
    return new InterestCalculationResult(0, accountId, calculationDate, false, errorMessage);
  }

  // Simplified static methods (your original ones)
  public static InterestCalculationResult Success(decimal interestAmount)
  {
    return new InterestCalculationResult(interestAmount, AccountId.Create(Guid.Empty), DateTime.UtcNow, true);
  }

  public static InterestCalculationResult Failure(string errorMessage)
  {
    return new InterestCalculationResult(0, AccountId.Create(Guid.Empty), DateTime.UtcNow, false, errorMessage);
  }

  // Alternative factory methods (more descriptive)
  public static InterestCalculationResult ForAccountSuccess(decimal interestAmount, AccountId accountId, DateTime calculationDate)
  {
    return new InterestCalculationResult(interestAmount, accountId, calculationDate, true);
  }

  public static InterestCalculationResult ForAccountFailure(string errorMessage, AccountId accountId, DateTime calculationDate)
  {
    return new InterestCalculationResult(0, accountId, calculationDate, false, errorMessage);
  }

  // Methods for batch operations
  public void AddSuccessfulAccount(decimal interestAmount, AccountId accountId)
  {
    SuccessfulCalculations++;
    TotalInterest += interestAmount;
    AccountDetails.Add(new AccountInterestDetail(accountId, interestAmount, true));
  }

  public void AddFailedAccount(string errorMessage, AccountId accountId)
  {
    FailedCalculations++;
    AccountDetails.Add(new AccountInterestDetail(accountId, 0, false, errorMessage));
  }

  // Helper method to check if batch was successful
  public bool IsBatchSuccess => FailedCalculations == 0;
}

// Helper class for account-level details
public class AccountInterestDetail
{
  public AccountId AccountId { get; }
  public decimal InterestAmount { get; }
  public bool IsSuccess { get; }
  public string ErrorMessage { get; }

  public AccountInterestDetail(AccountId accountId, decimal interestAmount, bool isSuccess, string errorMessage = "")
  {
    AccountId = accountId;
    InterestAmount = interestAmount;
    IsSuccess = isSuccess;
    ErrorMessage = errorMessage;
  }
}