namespace Whycespace.Platform.Api.Core.Contracts.Economic;

/// <summary>
/// Read-only settlement projection view.
/// Sourced from CQRS projections — no domain access, no aggregates.
/// </summary>
public sealed record SettlementView
{
    public required Guid SettlementId { get; init; }
    public required decimal Amount { get; init; }
    public required string Currency { get; init; }
    public required string Status { get; init; }
    public Guid? SourceAccountId { get; init; }
    public Guid? TargetAccountId { get; init; }
    public DateTimeOffset? SettledAt { get; init; }
}
