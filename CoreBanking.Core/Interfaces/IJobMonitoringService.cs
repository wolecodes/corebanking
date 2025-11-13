public interface IJobMonitoringService
{
  Task<JobStatistics> GetJobStatisticsAsync();
  Task<List<FailedJob>> GetRecentFailedJobsAsync(int count = 50);
  Task<bool> RetryFailedJobAsync(string jobId);
}

// CoreBanking.Core/Models
public class JobStatistics
{
  public int Servers { get; set; }
  public long Succeeded { get; set; }
  public long Failed { get; set; }
  public long Enqueued { get; set; }
  public long Processing { get; set; }
  public long Scheduled { get; set; }
  public long Deleted { get; set; }
  public long Recurring { get; set; }
  public long Retries { get; set; }
}

public class FailedJob
{
  public string JobId { get; set; } = string.Empty;
  public DateTime? FailedAt { get; set; }
  public string ExceptionMessage { get; set; } = string.Empty;
  public string ExceptionDetails { get; set; } = string.Empty;
}