using CoreBanking.Core.Entities;
using CoreBanking.Core.Enums;
using CoreBanking.Core.Events;
using CoreBanking.Core.Interfaces;
using CoreBanking.Core.ValueObjects;
using CoreBanking.APP.BackgroundJobs.Results;
using Hangfire;
using Microsoft.Extensions.Logging;



namespace CoreBanking.APP.BackgroundJobs

{

  public interface IInterestCalculationService
  {
    Task CalculateMonthlyInterestAsync(DateTime calculationDate, CancellationToken cancellationToken = default);
    Task<InterestCalculationResult> CalculateAccountInterestAsync(AccountId accountId, DateTime calculationDate, CancellationToken cancellationToken = default);
  }
  public class InterestCalculationService : IInterestCalculationService
  {
    private readonly IAccountRepository _accountRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly ILogger<InterestCalculationService> _logger;
    private readonly IEventPublisher _eventPublisher;

    public InterestCalculationService(
        IAccountRepository accountRepository,
        ITransactionRepository transactionRepository,
        ILogger<InterestCalculationService> logger,
        IEventPublisher eventPublisher)
    {
      _accountRepository = accountRepository;
      _transactionRepository = transactionRepository;
      _logger = logger;
      _eventPublisher = eventPublisher;
    }

    [AutomaticRetry(Attempts = 3, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
    public async Task CalculateMonthlyInterestAsync(DateTime calculationDate, CancellationToken cancellationToken = default)
    {
      _logger.LogInformation("Starting monthly interest calculation for {CalculationDate}", calculationDate.ToString("yyyy-MM-dd"));

      var startTime = DateTime.UtcNow;
      var interestBearingAccounts = await _accountRepository.GetInterestBearingAccountsAsync(cancellationToken);

      _logger.LogInformation("Found {AccountCount} interest-bearing accounts", interestBearingAccounts.Count);

      // Create batch result
      var batchResult = new InterestCalculationResult();
      var interestTransactions = new List<Transaction>();

      foreach (var account in interestBearingAccounts)
      {
        try
        {
          var accountResult = await CalculateAccountInterestAsync(account.AccountId, calculationDate, cancellationToken);

          if (accountResult.IsSuccess)
          {
            // Add to batch success
            batchResult.AddSuccessfulAccount(accountResult.InterestAmount, account.AccountId);

            var interestTransaction = Transaction.CreateInterestCredit(
                account.AccountId,
                accountResult.InterestAmount,
                $"Monthly interest for {calculationDate:MMMM yyyy}"
            );

            interestTransactions.Add(interestTransaction);
          }
          else
          {
            // Add to batch failures
            batchResult.AddFailedAccount(accountResult.ErrorMessage, account.AccountId);

            _logger.LogWarning("Failed to calculate interest for account {AccountId}: {Error}",
                account.AccountId, accountResult.ErrorMessage);
          }
        }
        catch (Exception ex)
        {
          // Add to batch failures with exception
          batchResult.AddFailedAccount(ex.Message, account.AccountId);
          _logger.LogError(ex, "Error calculating interest for account {AccountId}", account.AccountId);
        }
      }

      // Save all interest transactions
      if (interestTransactions.Any())
      {
        await _transactionRepository.AddRangeAsync(interestTransactions, cancellationToken);
        await _transactionRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created {TransactionCount} interest credit transactions", interestTransactions.Count);
      }

      var duration = DateTime.UtcNow - startTime;
      _logger.LogInformation("Completed interest calculation. Successful: {Success}, Failed: {Failed}, Total Interest: {TotalInterest:C}, Duration: {Duration}",
          batchResult.SuccessfulCalculations, batchResult.FailedCalculations, batchResult.TotalInterest, duration);

      // Publish interest calculation completed event
      await _eventPublisher.PublishAsync(new InterestCalculationCompletedEvent(
          calculationDate,
          batchResult.SuccessfulCalculations,
          batchResult.TotalInterest,
          duration));
    }

    // This method now returns the unified result type
    public async Task<InterestCalculationResult> CalculateAccountInterestAsync(Core.ValueObjects.AccountId accountId, DateTime calculationDate, CancellationToken cancellationToken = default)
    {
      var account = await _accountRepository.GetByIdAsync(accountId, cancellationToken);
      if (account == null)
        return InterestCalculationResult.Failure($"Account {accountId} not found", accountId, calculationDate);

      var interestBearingTypes = new[] { AccountType.Savings, AccountType.FixedDepost };
      if (!interestBearingTypes.Contains(account.AccountType))
        return InterestCalculationResult.Failure($"Account {accountId} is not interest-bearing", accountId, calculationDate);

      try
      {
        // Calculate average daily balance for the month
        var monthStart = new DateTime(calculationDate.Year, calculationDate.Month, 1);
        var monthEnd = monthStart.AddMonths(1).AddDays(-1);

        var averageBalance = await CalculateAverageDailyBalanceAsync(accountId, monthStart, monthEnd, cancellationToken);

        // Calculate interest based on account type and balance
        var interestRate = GetInterestRate(account.AccountType, averageBalance);
        var interestAmount = CalculateInterestAmount(averageBalance, interestRate, monthStart, monthEnd);

        _logger.LogDebug("Calculated interest for account {AccountId}: {InterestAmount:C} at rate {InterestRate:P2}",
            accountId, interestAmount, interestRate);

        return InterestCalculationResult.Success(interestAmount, accountId, calculationDate);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error calculating interest for account {AccountId}", accountId);
        return InterestCalculationResult.Failure(ex.Message, accountId, calculationDate);
      }
    }

    private async Task<decimal> CalculateAverageDailyBalanceAsync(Core.ValueObjects.AccountId accountId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
      // Implementation for average daily balance calculation
      // This would involve analyzing transaction history and daily balances
      await Task.CompletedTask;
      return 1000.00m; // Simplified for example
    }

    private decimal GetInterestRate(AccountType accountType, decimal balance)
    {
      return accountType switch
      {
        AccountType.Savings => balance >= 10000 ? 0.015m : 0.01m, // 1.5% or 1%
        AccountType.Checking => 0.001m, // 0.1%
        AccountType.FixedDepost => 0.035m, // 3.5%
        _ => 0.0m
      };
    }

    private decimal CalculateInterestAmount(decimal principal, decimal annualRate, DateTime startDate, DateTime endDate)
    {
      var daysInYear = 365;
      var days = (endDate - startDate).Days + 1;
      return principal * annualRate * days / daysInYear;
    }
  }
}
