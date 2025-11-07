namespace CoreBanking.APP.External.DTOs;
  public record CSCustomerValidationRequest
{
  public string CustomerId { get; init; } = string.Empty;
  public string FullName { get; init; } = string.Empty;
  public DateTime DateOfBirth { get; init; }
  public string BVN { get; init; } = string.Empty; // BVN, SSN, etc.
}