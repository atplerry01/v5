namespace Whycespace.Shared.Contracts.Domain.Economic;

/// <summary>
/// Evaluates enforcement decisions for economic operations.
/// Uses pre-fetched enforcement context (freeze, block, limit).
/// Output: Allow / Deny / Conditional.
/// </summary>
public interface IEconomicEnforcementEvaluator
{
    Task<EconomicEnforcementResult> EvaluateAsync(
        string identityId,
        string commandType,
        CancellationToken cancellationToken);
}

public sealed record EconomicEnforcementResult
{
    public required string Decision { get; init; }
    public string? ReasonCode { get; init; }

    public bool IsDenied => string.Equals(Decision, "Deny", StringComparison.OrdinalIgnoreCase);
    public bool IsConditional => string.Equals(Decision, "Conditional", StringComparison.OrdinalIgnoreCase);
    public bool IsAllowed => string.Equals(Decision, "Allow", StringComparison.OrdinalIgnoreCase);

    public static EconomicEnforcementResult Allow() => new() { Decision = "Allow" };
    public static EconomicEnforcementResult Deny(string reason) => new() { Decision = "Deny", ReasonCode = reason };
    public static EconomicEnforcementResult Conditional(string reason) => new() { Decision = "Conditional", ReasonCode = reason };
}
