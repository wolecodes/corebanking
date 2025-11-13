using Hangfire;
using Hangfire.Storage;

namespace CoreBanking.API.Services;

public class JobMonitoringService : IJobMonitoringService
{
  private readonly IMonitoringApi _monitoringApi;
  private readonly ILogger<JobMonitoringService> _logger;

  public JobMonitoringService(IMonitoringApi monitoringApi, ILogger<JobMonitoringService> logger)
  {
    _monitoringApi = monitoringApi;
    _logger = logger;
  }

  public async Task<JobStatistics> GetJobStatisticsAsync()
  {
    var stats = _monitoringApi.GetStatistics();

    return new JobStatistics
    {
      Servers = (int)stats.Servers,
      Succeeded = stats.Succeeded,
      Failed = stats.Failed,
      Enqueued = stats.Enqueued,
      Processing = stats.Processing,
      Scheduled = stats.Scheduled,
      Deleted = stats.Deleted,
      Recurring = stats.Recurring,
      Retries = stats.Retries ?? 0
    };
  }

  public async Task<List<FailedJob>> GetRecentFailedJobsAsync(int count = 50)
  {
    var failedJobs = new List<FailedJob>();
    var jobList = _monitoringApi.FailedJobs(0, count);

    foreach (var job in jobList)
    {
      failedJobs.Add(new FailedJob
      {
        JobId = job.Key,
        FailedAt = job.Value.FailedAt,
        ExceptionMessage = job.Value.ExceptionMessage,
        ExceptionDetails = job.Value.ExceptionDetails
      });
    }

    return failedJobs;
  }

  public async Task<bool> RetryFailedJobAsync(string jobId)
  {
    try
    {
      var job = _monitoringApi.JobDetails(jobId);
      if (job != null && job.History.Any(h => h.StateName == "Failed"))
      {
        BackgroundJob.Requeue(jobId);
        _logger.LogInformation("Requeued failed job {JobId}", jobId);
        return true;
      }
      return false;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to retry job {JobId}", jobId);
      return false;
    }
  }
}