namespace CoreBanking.Core.Interfaces;
public interface IJobInitializationService
{
  Task InitializeRecurringJobsAsync();
  Task RegisterOneTimeJobsAsync();
}