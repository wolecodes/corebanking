namespace CoreBanking.Core.Models;

public record SimulatedBVNResponse
{
  public string BVN { get; init; } = string.Empty;
  public bool IsValid { get; init; }
  public DateTime ValidationDate { get; init; }
  public string Message { get; init; } = string.Empty;
}