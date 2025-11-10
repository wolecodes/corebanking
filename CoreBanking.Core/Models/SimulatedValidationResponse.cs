namespace CoreBanking.Core.Models;
public record SimulatedValidationResponse
{
  public bool IsValid { get; init; }
  public string Reason { get; init; } = string.Empty;
  public DateTime ValidatedAt { get; init; }
}
