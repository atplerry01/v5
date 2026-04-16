namespace Whycespace.Shared.Contracts.Economic.Reconciliation.Discrepancy;

public sealed record DiscrepancyReadModel
{
    public Guid DiscrepancyId { get; init; }
    public Guid ProcessReference { get; init; }
    public string Source { get; init; } = string.Empty;
    public decimal ExpectedValue { get; init; }
    public decimal ActualValue { get; init; }
    public decimal Difference { get; init; }
    public string Status { get; init; } = string.Empty;
    public string Resolution { get; init; } = string.Empty;
    public DateTimeOffset DetectedAt { get; init; }
    public DateTimeOffset LastUpdatedAt { get; init; }
}
