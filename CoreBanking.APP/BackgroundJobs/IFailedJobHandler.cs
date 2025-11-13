using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using CoreBanking.Core.Interfaces;
using Hangfire.Common;
using Hangfire.States;
using Microsoft.Data.SqlClient;

namespace CoreBanking.APP.BackgroundJobs;

public interface IFailedJobHandler
{
  Task HandleFailedStatementJobAsync(string jobId, Exception exception);
  Task HandleFailedInterestJobAsync(string jobId, Exception exception);
  Task<bool> CanRecoverJobAsync(string jobId, Exception exception);
}

public class FailedJobHandler : IFailedJobHandler
{
  private readonly ILogger<FailedJobHandler> _logger;
  // private readonly IEmailService _emailService;
  private readonly IHangfireService _hangfireService;

  public FailedJobHandler(
      ILogger<FailedJobHandler> logger,
      // IEmailService emailService,
      IHangfireService hangfireService)
  {
    _logger = logger;
    // _emailService = emailService;
    _hangfireService = hangfireService;
  }

  public async Task HandleFailedStatementJobAsync(string jobId, Exception exception)
  {
    _logger.LogError(exception, "Statement generation job {JobId} failed", jobId);

    // Send alert to operations team
    // await _emailService.SendJobFailureAlertAsync(
    //     "Statement Generation Failure",
    //     $"Job {jobId} failed: {exception.Message}",
    //     exception.ToString());

    // Check if this is a recoverable failure
    if (await CanRecoverJobAsync(jobId, exception))
    {
      _logger.LogInformation("Attempting to recover statement job {JobId}", jobId);
      await _hangfireService.TriggerJobAsync<DailyStatementService>(
          x => x.GenerateDailyStatementsAsync(DateTime.UtcNow.Date.AddDays(-1), CancellationToken.None));
    }
  }

  public async Task HandleFailedInterestJobAsync(string jobId, Exception exception)
  {
    _logger.LogError(exception, "Interest calculation job {JobId} failed", jobId);

    // This is critical - interest calculation failures need immediate attention
    // await _emailService.SendCriticalAlertAsync(
    //     "CRITICAL: Interest Calculation Failure",
    //     $"Job {jobId} failed: {exception.Message}",
    //     exception.ToString());

    // For interest jobs, always attempt recovery due to financial impact
    _logger.LogWarning("Attempting immediate recovery of interest calculation job {JobId}", jobId);
    await _hangfireService.TriggerJobAsync<InterestCalculationService>(
        x => x.CalculateMonthlyInterestAsync(DateTime.UtcNow.Date, CancellationToken.None));
  }

  public async Task<bool> CanRecoverJobAsync(string jobId, Exception exception)
  {
    // Determine if the job failure is recoverable based on exception type
    return exception switch
    {
      TimeoutException => true,
      HttpRequestException => true,
      SqlException sqlEx when sqlEx.Number == -2 => true, // SQL timeout
      SqlException sqlEx when sqlEx.Number == 1205 => true, // Deadlock
      _ => false
    };
  }
}

// Custom job filter for enhanced error handling
public class BankingJobFilter : JobFilterAttribute, IElectStateFilter
{
  private readonly IServiceProvider _serviceProvider;

  public BankingJobFilter(IServiceProvider serviceProvider)
  {
    _serviceProvider = serviceProvider;
  }

  public void OnStateElection(ElectStateContext context)
  {
    if (context.CandidateState is FailedState failedState)
    {
      using var scope = _serviceProvider.CreateScope();
      var failedJobHandler = scope.ServiceProvider.GetRequiredService<IFailedJobHandler>();

      var jobType = context.BackgroundJob.Job.Type.Name;
      var jobId = context.BackgroundJob.Id;
      var exception = failedState.Exception;

      // Route to appropriate handler based on job type
      if (jobType.Contains("Statement"))
      {
        Task.Run(() => failedJobHandler.HandleFailedStatementJobAsync(jobId, exception));
      }
      else if (jobType.Contains("Interest"))
      {
        Task.Run(() => failedJobHandler.HandleFailedInterestJobAsync(jobId, exception));
      }
    }
  }
}