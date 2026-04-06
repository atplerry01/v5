namespace Whycespace.Platform.Api.Core.Contracts.Graph;

/// <summary>
/// Read-only capital flow link between two SPVs.
/// Sourced from pre-built graph projections — no aggregation or calculation.
/// </summary>
public sealed record SpvFlowView
{
    public required Guid FromSpvId { get; init; }
    public required Guid ToSpvId { get; init; }
    public required decimal Amount { get; init; }
    public required string Currency { get; init; }
    public string? FlowType { get; init; }
    public DateTimeOffset? LastFlowAt { get; init; }
}
