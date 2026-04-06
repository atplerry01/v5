namespace Whycespace.Platform.Api.Core.Contracts.Economic;

/// <summary>
/// Read-only revenue projection view.
/// Sourced from CQRS projections — no domain access, no aggregates.
/// </summary>
public sealed record RevenueView
{
    public required Guid RevenueId { get; init; }
    public required decimal Amount { get; init; }
    public required string Currency { get; init; }
    public required string Status { get; init; }
    public string? Source { get; init; }
    public DateTimeOffset? RecognizedAt { get; init; }
}
