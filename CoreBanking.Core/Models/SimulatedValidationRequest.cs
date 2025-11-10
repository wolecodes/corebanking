namespace CoreBanking.Core.Models;

public record SimulatedValidationRequest
{
  public string BVN { get; init; } = string.Empty;
  public string FullName { get; init; } = string.Empty;
  public DateTime DateOfBirth { get; init; }
}
