namespace CoreBanking.Core.Models;

public record SimulatedCreditScoreResponse
{
  public string BVN { get; init; } = string.Empty;
  public int Score { get; init; }
  public string Band { get; init; } = string.Empty;
  public string[] Factors { get; init; } = Array.Empty<string>();
  public DateTime GeneratedAt { get; init; }
  public bool IsSuccess { get; init; }
  public string ErrorMessage { get; init; } = string.Empty;
}
