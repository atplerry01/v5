namespace Whycespace.Shared.Contracts.Control.SystemReconciliation.DiscrepancyResolution;

public sealed record DiscrepancyResolutionReadModel
{
    public Guid ResolutionId { get; init; }
    public string DetectionId { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset InitiatedAt { get; init; }
    public string? Outcome { get; init; }
    public string? Notes { get; init; }
    public DateTimeOffset? CompletedAt { get; init; }
}
