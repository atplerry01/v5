using Whycespace.Shared.Primitives.Money;

namespace Whycespace.Shared.Contracts.Policy;

/// <summary>
/// Domain-specific policy input for distribution operations.
/// Immutable. Uses Money (not decimal).
/// MUST be constructed and passed before policy evaluation -- no fallback to partial input.
/// </summary>
public sealed record DistributionPolicyInput
{
    public required string IdentityId { get; init; }
    public required Guid RevenueId { get; init; }
    public required Guid TransactionId { get; init; }
    public required Money Amount { get; init; }
    public required string Jurisdiction { get; init; }
}
