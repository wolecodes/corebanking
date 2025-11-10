using CoreBanking.Core.Models;

namespace CoreBanking.Core.Interfaces
{
  public interface ISimulatedCreditScoringService
  {
    Task<SimulatedCreditScoreResponse> GetCreditScoreAsync(string bvn, CancellationToken cancellationToken = default);
    Task<SimulatedCreditReportResponse> GetCreditReportAsync(string bvn, CancellationToken cancellationToken = default);
    Task<SimulatedValidationResponse> ValidateCustomerAsync(SimulatedValidationRequest request, CancellationToken cancellationToken = default);
    Task<SimulatedBVNResponse> ValidateBVNAsync(string bvn, CancellationToken cancellationToken = default);
  }
}