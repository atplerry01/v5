namespace Whycespace.Shared.Contracts.Control.SystemReconciliation.DiscrepancyDetection;

public sealed record DiscrepancyDetectionReadModel
{
    public Guid DetectionId { get; init; }
    public string Kind { get; init; } = string.Empty;
    public string SourceReference { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset DetectedAt { get; init; }
    public string? DismissalReason { get; init; }
}
