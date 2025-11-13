using CoreBanking.Core.Entities;
using Microsoft.Extensions.Logging;
using CoreBanking.Core.ValueObjects;
using CoreBanking.Core.Interfaces;
using Hangfire;
namespace CoreBanking.APP.BackgroundJobs;

public interface IDailyStatementService
{
  Task GenerateDailyStatementsAsync(DateTime statementDate, CancellationToken cancellationToken = default);
  Task<StatementGenerationResult> GenerateCustomerStatementAsync(Guid customerId, DateTime statementDate, CancellationToken cancellationToken = default);
}

public class DailyStatementService : IDailyStatementService
{
  private readonly IAccountRepository _accountRepository;
  private readonly ITransactionRepository _transactionRepository;
  private readonly ILogger<DailyStatementService> _logger;
  // private readonly IPdfGenerationService _pdfGenerationService;
  // private readonly IEmailService _emailService;

  public DailyStatementService(
      IAccountRepository accountRepository,
      ITransactionRepository transactionRepository,
      ILogger<DailyStatementService> logger)
  {
    _accountRepository = accountRepository;
    _transactionRepository = transactionRepository;
    _logger = logger;
    // _pdfGenerationService = pdfGenerationService;
    // _emailService = emailService;
  }

  [AutomaticRetry(Attempts = 3, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
  public async Task GenerateDailyStatementsAsync(DateTime statementDate, CancellationToken cancellationToken = default)
  {
    _logger.LogInformation("Starting daily statement generation for {StatementDate}", statementDate.ToString("yyyy-MM-dd"));

    var startTime = DateTime.UtcNow;
    var activeAccounts = await _accountRepository.GetActiveAccountsAsync(cancellationToken);

    _logger.LogInformation("Found {AccountCount} active accounts for statement generation", activeAccounts.Count);

    var results = new StatementGenerationResult();

    // Process in batches to avoid memory issues
    const int batchSize = 100;
    for (int i = 0; i < activeAccounts.Count; i += batchSize)
    {
      var batch = activeAccounts.Skip(i).Take(batchSize).ToList();
      var batchTasks = batch.Select(account =>
          GenerateAccountStatementAsync(account.AccountId, statementDate, cancellationToken));

      var batchResults = await Task.WhenAll(batchTasks);
      results.ProcessedAccounts += batchResults.Count(r => r.IsSuccess);
      results.FailedAccounts += batchResults.Count(r => !r.IsSuccess);

      _logger.LogInformation("Processed batch {BatchNumber}, Success: {SuccessCount}, Failed: {FailedCount}",
          (i / batchSize) + 1, results.ProcessedAccounts, results.FailedAccounts);
    }

    var duration = DateTime.UtcNow - startTime;
    _logger.LogInformation("Completed daily statement generation. Processed: {Processed}, Failed: {Failed}, Duration: {Duration}",
        results.ProcessedAccounts, results.FailedAccounts, duration);
  }

  private async Task<AccountStatementResult> GenerateAccountStatementAsync(AccountId accountId, DateTime statementDate, CancellationToken cancellationToken)
  {
    try
    {
      var account = await _accountRepository.GetByIdAsync(accountId, cancellationToken);
      if (account == null)
      {
        _logger.LogWarning("Account {AccountId} not found for statement generation", accountId);
        return AccountStatementResult.Failure($"Account {accountId} not found");
      }

      // Get transactions for the statement period
      var startDate = statementDate.AddDays(-30); // Monthly statements
      var endDate = statementDate;

      var transactions = await _transactionRepository.GetTransactionsByAccountAndDateRangeAsync(
          accountId, startDate, endDate, cancellationToken);

      // Generate PDF statement
      // var statementPdf = await _pdfGenerationService.GenerateAccountStatementAsync(
      //     account, transactions, startDate, endDate, cancellationToken);

      // Store statement in database or file storage
      // await StoreStatementAsync(accountId, statementDate, statementPdf, cancellationToken);

      // // Send email notification if customer opted in
      // if (account.Customer.EmailOptIn)
      // {
      //   await _emailService.SendStatementNotificationAsync(
      //       account.Customer.Email,
      //       account.Customer.FullName,
      //       statementDate,
      //       statementPdf,
      //       cancellationToken);
      // }

      return AccountStatementResult.Success(accountId);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to generate statement for account {AccountId}", accountId);
      return AccountStatementResult.Failure(ex.Message);
    }
  }

  public async Task<StatementGenerationResult> GenerateCustomerStatementAsync(Guid customerId, DateTime statementDate, CancellationToken cancellationToken = default)
  {
    // Implementation for individual customer statement generation
    await Task.CompletedTask;
    return new StatementGenerationResult();
  }

  private async Task StoreStatementAsync(AccountId accountId, DateTime statementDate, byte[] statementPdf, CancellationToken cancellationToken)
  {
    // Implementation for storing generated statements
    await Task.CompletedTask;
  }
}
public class StatementGenerationResult
{
  public int ProcessedAccounts { get; set; }
  public int FailedAccounts { get; set; }
  public bool IsSuccess => FailedAccounts == 0;
  public TimeSpan Duration { get; set; }
}

public class AccountStatementResult
{
  public bool IsSuccess { get; private set; }
  public string ErrorMessage { get; private set; }
  public AccountId AccountId { get; private set; }

  public static AccountStatementResult Success(AccountId accountId)
      => new AccountStatementResult { IsSuccess = true, AccountId = accountId };

  public static AccountStatementResult Failure(string errorMessage)
      => new AccountStatementResult { IsSuccess = false, ErrorMessage = errorMessage };
}