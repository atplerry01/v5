namespace Whycespace.Platform.Api.Core.Contracts.Economic;

/// <summary>
/// Read-only distribution projection view.
/// Sourced from CQRS projections — no domain access, no aggregates.
/// </summary>
public sealed record DistributionView
{
    public required Guid DistributionId { get; init; }
    public required decimal TotalAmount { get; init; }
    public required string Currency { get; init; }
    public required string Status { get; init; }
    public int RecipientCount { get; init; }
    public DateTimeOffset? DistributedAt { get; init; }
}
