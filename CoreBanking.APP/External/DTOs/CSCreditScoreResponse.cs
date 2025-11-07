namespace CoreBanking.APP.External.DTOs;

public record CSCreditScoreResponse
{
  public string CustomerId { get; init; } = string.Empty;
  public int Score { get; init; }
  public string Band { get; init; } = string.Empty; // "Poor", "Fair", "Good", "Excellent"
  public DateTime GeneratedAt { get; init; }
  public string[] Factors { get; init; } = Array.Empty<string>();
  public bool IsSuccess { get; init; }
  public string ErrorMessage { get; init; } = string.Empty;

  public static CSCreditScoreResponse Failed(string error) => new()
  {
    IsSuccess = false,
    ErrorMessage = error
  };
}