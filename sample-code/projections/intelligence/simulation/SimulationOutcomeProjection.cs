namespace Whycespace.Projections.PolicySimulation;

/// <summary>
/// Read model for simulation calibration data.
/// Stores predicted vs actual outcomes for accuracy tracking.
/// Written to analytics/projection store ONLY — never to transactional DB.
/// </summary>
public sealed record SimulationOutcomeProjection
{
    public required Guid Id { get; init; }
    public required Guid PolicyId { get; init; }
    public required int Version { get; init; }
    public required string SimulationResult { get; init; }
    public string? ActualOutcome { get; init; }
    public double? DeviationScore { get; init; }
    public required Guid SnapshotId { get; init; }
    public required DateTimeOffset RecordedAt { get; init; }
    public DateTimeOffset? ActualRecordedAt { get; init; }
}

/// <summary>
/// Aggregated calibration metrics per policy.
/// </summary>
public sealed record CalibrationMetricsProjection
{
    public required Guid PolicyId { get; init; }
    public required int SampleCount { get; init; }
    public required double MeanDeviation { get; init; }
    public required double MedianDeviation { get; init; }
    public required double MaxDeviation { get; init; }
    public required double AccuracyRate { get; init; }
    public required DateTimeOffset ComputedAt { get; init; }
}
