namespace Whycespace.Shared.Contracts.Domain.Economic;

/// <summary>
/// Engine-facing revenue invariant validation contract.
/// Replaces direct import of domain RevenueInvariantService.
/// </summary>
public interface IRevenueInvariantValidator
{
    RevenueValidationResult Validate(object totalAmount, object recognizedAmount, object obligationStatus);
}

public sealed record RevenueValidationResult(bool IsValid, string? Error)
{
    public static RevenueValidationResult Fail(string error) => new(false, error);
    public static RevenueValidationResult Success() => new(true, null);
}
