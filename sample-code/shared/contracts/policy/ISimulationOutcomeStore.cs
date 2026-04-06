namespace Whycespace.Shared.Contracts.Policy;

/// <summary>
/// Read/write contract for simulation calibration projections.
/// Backed by analytics/projection store — NOT the transactional DB.
/// </summary>
public interface ISimulationOutcomeStore
{
    Task RecordOutcomeAsync(SimulationOutcomeRecord record, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SimulationOutcomeRecord>> GetOutcomesByPolicyAsync(Guid policyId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SimulationOutcomeRecord>> GetOutcomesInRangeAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SimulationOutcomeRecord>> GetRecentOutcomesAsync(Guid policyId, int count, CancellationToken cancellationToken = default);
}

public sealed record SimulationOutcomeRecord
{
    public required Guid Id { get; init; }
    public required Guid PolicyId { get; init; }
    public required int Version { get; init; }
    public required string SimulationResult { get; init; }
    public string? ActualOutcome { get; init; }
    public double? DeviationScore { get; init; }
    public required Guid SnapshotId { get; init; }
    public required DateTimeOffset RecordedAt { get; init; }
}