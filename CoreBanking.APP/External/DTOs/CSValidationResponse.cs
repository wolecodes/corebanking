namespace CoreBanking.APP.External.DTOs;

public record CSValidationResponse
{
  public bool IsValid { get; init; }
  public string Reason { get; init; } = string.Empty;
}