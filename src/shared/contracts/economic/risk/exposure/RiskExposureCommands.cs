using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Economic.Risk.Exposure;

public sealed record CreateRiskExposureCommand(
    Guid ExposureId,
    Guid SourceId,
    int ExposureType,
    decimal InitialExposure,
    string Currency,
    DateTimeOffset CreatedAt) : IHasAggregateId
{
    public Guid AggregateId => ExposureId;
}

public sealed record IncreaseRiskExposureCommand(
    Guid ExposureId,
    decimal Amount) : IHasAggregateId
{
    public Guid AggregateId => ExposureId;
}

public sealed record ReduceRiskExposureCommand(
    Guid ExposureId,
    decimal Amount) : IHasAggregateId
{
    public Guid AggregateId => ExposureId;
}

public sealed record CloseRiskExposureCommand(
    Guid ExposureId) : IHasAggregateId
{
    public Guid AggregateId => ExposureId;
}

/// <summary>
/// Phase 6 T6.5 — evaluates the current TotalExposure on the aggregate
/// against the supplied threshold and raises <c>ExposureBreachedEvent</c>
/// when breached. The emission feeds the enforcement pipeline via
/// <c>RiskExposureEnforcementHandler</c>, which dispatches a
/// <c>DetectViolationCommand</c> so breach → enforcement is
/// deterministic and loop-closed.
/// </summary>
public sealed record DetectRiskExposureBreachCommand(
    Guid ExposureId,
    decimal Threshold,
    DateTimeOffset DetectedAt) : IHasAggregateId
{
    public Guid AggregateId => ExposureId;
}
