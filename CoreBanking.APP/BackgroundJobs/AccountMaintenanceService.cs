using CoreBanking.Core.Entities;
using CoreBanking.Core.Interfaces;
using CoreBanking.Application.Common.Models;
using Microsoft.Extensions.Logging;
using Hangfire;

namespace CoreBanking.Application.BackgroundJobs;

public class AccountMaintenanceService : IAccountMaintenanceService
{
  private readonly IAccountRepository _accountRepository;
  private readonly ITransactionRepository _transactionRepository;
  private readonly ILogger<AccountMaintenanceService> _logger;
  private readonly IEventPublisher _eventPublisher;

  public AccountMaintenanceService(
      IAccountRepository accountRepository,
      ITransactionRepository transactionRepository,
      ILogger<AccountMaintenanceService> logger,
      IEventPublisher eventPublisher)
  {
    _accountRepository = accountRepository;
    _transactionRepository = transactionRepository;
    _logger = logger;
    _eventPublisher = eventPublisher;
  }

  [AutomaticRetry(Attempts = 3)]
  public async Task<AccountMaintenanceResult> CleanupInactiveAccountsAsync(CancellationToken cancellationToken = default)
  {
    _logger.LogInformation("Starting inactive account cleanup");

    var startTime = DateTime.UtcNow;
    var result = new AccountMaintenanceResult();
    var operationDetails = new List<MaintenanceOperationDetail>();

    try
    {
      // Get accounts inactive for more than 2 years
      var cutoffDate = DateTime.UtcNow.AddYears(-2);
      var inactiveAccounts = await _accountRepository.GetInactiveAccountsSinceAsync(cutoffDate, cancellationToken);

      _logger.LogInformation("Found {Count} inactive accounts for cleanup", inactiveAccounts.Count);

      foreach (var account in inactiveAccounts)
      {
        var operationDetail = new MaintenanceOperationDetail
        {
          OperationType = "AccountCleanup",
          AccountId = account.AccountId.Value.ToString()
        };

        try
        {
          // Check if account has zero balance and no recent activity
          if (await CanCloseAccountAsync(account, cancellationToken))
          {
            // Mark account as closed
            account.MarkAsClosed();
            await _accountRepository.UpdateAsync(account);

            operationDetail.IsSuccess = true;
            operationDetail.Message = "Account closed successfully";
            result.SuccessfulOperations++;
          }
          else
          {
            operationDetail.IsSuccess = true;
            operationDetail.Message = "Account not eligible for closure (non-zero balance or recent activity)";
            result.SuccessfulOperations++;
          }
        }
        catch (Exception ex)
        {
          operationDetail.IsSuccess = false;
          operationDetail.Message = $"Failed to close account: {ex.Message}";
          result.FailedOperations++;
          _logger.LogError(ex, "Failed to cleanup account {AccountId}", account.AccountId);
        }

        operationDetails.Add(operationDetail);
        result.ProcessedCount++;
      }

      // Save all changes
      if (inactiveAccounts.Any())
      {
        await _accountRepository.SaveChangesAsync(cancellationToken);
      }

      result.OperationDetails = operationDetails;
      result.Duration = DateTime.UtcNow - startTime;

      _logger.LogInformation("Inactive account cleanup completed. Processed: {Processed}, Successful: {Successful}, Failed: {Failed}",
          result.ProcessedCount, result.SuccessfulOperations, result.FailedOperations);

      return result;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to complete inactive account cleanup");
      throw;
    }
  }

  [AutomaticRetry(Attempts = 3)]
  public async Task<AccountMaintenanceResult> ArchiveOldTransactionsAsync(DateTime cutoffDate, CancellationToken cancellationToken = default)
  {
    _logger.LogInformation("Starting transaction archiving for transactions before {CutoffDate}", cutoffDate);

    var startTime = DateTime.UtcNow;
    var result = new AccountMaintenanceResult();

    try
    {
      // Get old transactions
      var oldTransactions = await _transactionRepository.GetTransactionsBeforeAsync(cutoffDate, cancellationToken);

      _logger.LogInformation("Found {Count} transactions to archive", oldTransactions.Count);

      // In a real implementation, you would:
      // 1. Export transactions to archive storage (data lake, cold storage, etc.)
      // 2. Remove from active database (or mark as archived)
      // 3. Update audit records

      // For now, we'll just mark them as archived in the database
      foreach (var transaction in oldTransactions)
      {
        transaction.MarkAsArchived();
      }

      if (oldTransactions.Any())
      {
        await _transactionRepository.SaveChangesAsync(cancellationToken);
      }

      result.ProcessedCount = oldTransactions.Count;
      result.SuccessfulOperations = oldTransactions.Count;
      result.Duration = DateTime.UtcNow - startTime;

      _logger.LogInformation("Transaction archiving completed. Archived {Count} transactions", oldTransactions.Count);

      return result;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to archive transactions");
      throw;
    }
  }

  [AutomaticRetry(Attempts = 2)]
  public async Task<AccountMaintenanceResult> ValidateAccountDataAsync(CancellationToken cancellationToken = default)
  {
    _logger.LogInformation("Starting account data validation");

    var startTime = DateTime.UtcNow;
    var result = new AccountMaintenanceResult();
    var operationDetails = new List<MaintenanceOperationDetail>();

    try
    {
      var allAccounts = await _accountRepository.GetAllAsync();

      foreach (var account in allAccounts)
      {
        var operationDetail = new MaintenanceOperationDetail
        {
          OperationType = "DataValidation",
          AccountId = account.AccountId.Value.ToString()
        };

        try
        {
          var validationErrors = ValidateAccount(account);

          if (!validationErrors.Any())
          {
            operationDetail.IsSuccess = true;
            operationDetail.Message = "Account data is valid";
            result.SuccessfulOperations++;
          }
          else
          {
            operationDetail.IsSuccess = false;
            operationDetail.Message = $"Validation errors: {string.Join(", ", validationErrors)}";
            result.FailedOperations++;

            _logger.LogWarning("Account {AccountId} validation failed: {Errors}",
                account.AccountId, string.Join(", ", validationErrors));
          }
        }
        catch (Exception ex)
        {
          operationDetail.IsSuccess = false;
          operationDetail.Message = $"Validation error: {ex.Message}";
          result.FailedOperations++;
          _logger.LogError(ex, "Failed to validate account {AccountId}", account.AccountId);
        }

        operationDetails.Add(operationDetail);
        result.ProcessedCount++;
      }

      result.OperationDetails = operationDetails;
      result.Duration = DateTime.UtcNow - startTime;

      _logger.LogInformation("Account data validation completed. Processed: {Processed}, Valid: {Successful}, Invalid: {Failed}",
          result.ProcessedCount, result.SuccessfulOperations, result.FailedOperations);

      return result;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to complete account data validation");
      throw;
    }
  }

  [AutomaticRetry(Attempts = 3)]
  public async Task<AccountMaintenanceResult> UpdateAccountStatusesAsync(CancellationToken cancellationToken = default)
  {
    _logger.LogInformation("Starting account status updates");

    var startTime = DateTime.UtcNow;
    var result = new AccountMaintenanceResult();

    try
    {
      var accounts = await _accountRepository.GetAllAsync();
      var updatedCount = 0;

      foreach (var account in accounts)
      {
        try
        {
          var originalStatus = account.IsActive ? "Active" : "Inactive";

          // Update status based on business rules
          if (ShouldUpdateAccountStatus(account))
          {
            account.UpdateStatusBasedOnRules();
            await _accountRepository.UpdateAsync(account);
            updatedCount++;

            var newStatus = account.IsActive ? "Active" : "Inactive";
            _logger.LogDebug("Updated account {AccountId} status from {OldStatus} to {NewStatus}",
                account.AccountId, originalStatus, newStatus);
          }
        }
        catch (Exception ex)
        {
          _logger.LogError(ex, "Failed to update status for account {AccountId}", account.AccountId);
          result.FailedOperations++;
        }

        result.ProcessedCount++;
      }

      if (updatedCount > 0)
      {
        await _accountRepository.SaveChangesAsync(cancellationToken);
      }

      result.SuccessfulOperations = result.ProcessedCount - result.FailedOperations;
      result.Duration = DateTime.UtcNow - startTime;

      _logger.LogInformation("Account status updates completed. Processed: {Processed}, Updated: {Updated}, Failed: {Failed}",
          result.ProcessedCount, updatedCount, result.FailedOperations);

      return result;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to update account statuses");
      throw;
    }
  }

  private async Task<bool> CanCloseAccountAsync(Account account, CancellationToken cancellationToken)
  {
    // Check if account has zero balance
    if (account.Balance.Amount > 0)
      return false;

    // Check for recent transactions (last 6 months)
    var recentTransactions = await _transactionRepository.GetRecentTransactionsByAccountAsync(
        account.AccountId, DateTime.UtcNow.AddMonths(-6), cancellationToken);

    return !recentTransactions.Any();
  }

  private List<string> ValidateAccount(Account account)
  {
    var errors = new List<string>();

    // Basic validation rules
    if (string.IsNullOrWhiteSpace(account.AccountNumber.Value))
      errors.Add("Account number is required");

    if (account.Balance.Amount < 0)
      errors.Add("Account balance cannot be negative");

    if (account.Customer == null)
      errors.Add("Account must have a customer");

    // Add more validation rules as needed

    return errors;
  }

  private bool ShouldUpdateAccountStatus(Account account)
  {
    // Implement business rules for status updates
    // Example: If account has been dormant for 1 year, mark as inactive
    // Example: If account has suspicious activity, mark for review

    // Placeholder logic
    //return account.LastActivityDate < DateTime.UtcNow.AddYears(-1);
    return account.DeletedAt < DateTime.UtcNow.AddYears(-1);
  }
}