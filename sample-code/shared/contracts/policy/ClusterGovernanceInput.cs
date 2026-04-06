namespace Whycespace.Shared.Contracts.Policy;

/// <summary>
/// E18.7.2 — Policy input for cluster governance decisions.
/// </summary>
public sealed record ClusterGovernanceInput
{
    public required Guid ClusterId { get; init; }
    public required string DecisionType { get; init; }
    public required decimal EconomicImpact { get; init; }
    public required string Jurisdiction { get; init; }
    public required string IdentityId { get; init; }
}
