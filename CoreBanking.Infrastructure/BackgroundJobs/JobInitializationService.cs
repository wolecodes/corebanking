using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CoreBanking.Core.Interfaces;
using CoreBanking.APP.BackgroundJobs;
using CoreBanking.APP.Common.Models;
using CoreBanking.Application.BackgroundJobs;

namespace CoreBanking.Infrastructure.BackgroundJobs;

public class JobInitializationService : IJobInitializationService
{
  private readonly IHangfireService _hangfireService;
  private readonly HangfireConfiguration _config;
  private readonly ILogger<JobInitializationService> _logger;
  private readonly IServiceProvider _serviceProvider;

  public JobInitializationService(
      IHangfireService hangfireService,
      IOptions<HangfireConfiguration> config,
      ILogger<JobInitializationService> logger,
      IServiceProvider serviceProvider)
  {
    _hangfireService = hangfireService;
    _config = config.Value;
    _logger = logger;
    _serviceProvider = serviceProvider;
  }

  public async Task InitializeRecurringJobsAsync()
  {
    _logger.LogInformation("Initializing recurring jobs");

    try
    {
      // Daily Statement Generation
      await _hangfireService.ScheduleRecurringJobAsync<DailyStatementService>(
          "DailyStatementGeneration",
          x => x.GenerateDailyStatementsAsync(DateTime.UtcNow.Date, CancellationToken.None),
          _config.ScheduledJobs["DailyStatementGeneration"]);

      // Monthly Interest Calculation
      await _hangfireService.ScheduleRecurringJobAsync<InterestCalculationService>(
          "MonthlyInterestCalculation",
          x => x.CalculateMonthlyInterestAsync(DateTime.UtcNow.Date, CancellationToken.None),
          _config.ScheduledJobs["MonthlyInterestCalculation"]);

      // Account Cleanup
      await _hangfireService.ScheduleRecurringJobAsync<AccountMaintenanceService>(
          "AccountCleanup",
          x => x.CleanupInactiveAccountsAsync(CancellationToken.None),
          _config.ScheduledJobs["AccountCleanup"]);

      _logger.LogInformation("Successfully initialized all recurring jobs");
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to initialize recurring jobs");
      throw;
    }
  }

  public async Task RegisterOneTimeJobsAsync()
  {
    _logger.LogInformation("Registering one-time jobs");

    // Example: Schedule end-of-month reporting
    // var endOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1)
    //     .AddMonths(1)
    //     .AddDays(-1);

    // await _hangfireService.ScheduleJobAsync<MonthlyReportService>(
    //     x => x.GenerateMonthlyReportsAsync(endOfMonth, CancellationToken.None),
    //     endOfMonth - DateTime.UtcNow);

    _logger.LogInformation("Successfully registered one-time jobs");
    await Task.CompletedTask;
  }
}