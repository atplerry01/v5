namespace Whycespace.Shared.Contracts.Domain.Economic;

/// <summary>
/// Evaluates WhycePolicy for economic operations.
/// Input: transaction amount, trust score, identity status.
/// Output: Allow / Deny / Conditional with reason.
/// </summary>
public interface IEconomicPolicyEvaluator
{
    Task<EconomicPolicyResult> EvaluateAsync(
        EconomicPolicyInput input,
        CancellationToken cancellationToken);
}

public sealed record EconomicPolicyInput
{
    public required string IdentityId { get; init; }
    public required string CommandType { get; init; }
    public decimal? TransactionAmount { get; init; }
    public decimal TrustScore { get; init; }
    public string? FederationStatus { get; init; }
    public string? CorrelationId { get; init; }
}

public sealed record EconomicPolicyResult
{
    public required string Decision { get; init; }
    public string? ReasonCode { get; init; }
    public IReadOnlyList<string> Conditions { get; init; } = [];

    public bool IsDenied => string.Equals(Decision, "Deny", StringComparison.OrdinalIgnoreCase);
    public bool IsConditional => string.Equals(Decision, "Conditional", StringComparison.OrdinalIgnoreCase);
    public bool IsAllowed => string.Equals(Decision, "Allow", StringComparison.OrdinalIgnoreCase);

    public static EconomicPolicyResult Allow() => new() { Decision = "Allow" };
    public static EconomicPolicyResult Deny(string reason) => new() { Decision = "Deny", ReasonCode = reason };
    public static EconomicPolicyResult Conditional(string reason, IReadOnlyList<string> conditions) =>
        new() { Decision = "Conditional", ReasonCode = reason, Conditions = conditions };
}
