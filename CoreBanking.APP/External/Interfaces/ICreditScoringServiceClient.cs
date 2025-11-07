using CoreBanking.APP.External.DTOs;
namespace CoreBanking.APP.External.Interfaces;


public interface ICreditScoringServiceClient
{
  Task<CSCreditScoreResponse> GetCreditScoreAsync(string customerId, CancellationToken cancellationToken = default);
  Task<CSCreditReportResponse> GetCreditReportAsync(string customerId, CancellationToken cancellationToken = default);
  Task<bool> ValidateCustomerAsync(CSCustomerValidationRequest request, CancellationToken cancellationToken = default);
}
