namespace Whycespace.Shared.Contracts.Events.Economic.Risk.Exposure;

public sealed record RiskExposureCreatedEventSchema(
    Guid AggregateId,
    Guid SourceId,
    int ExposureType,
    decimal TotalExposure,
    string Currency,
    DateTimeOffset CreatedAt);

public sealed record RiskExposureIncreasedEventSchema(
    Guid AggregateId,
    decimal IncreasedBy,
    decimal NewTotal);

public sealed record RiskExposureReducedEventSchema(
    Guid AggregateId,
    decimal ReducedBy,
    decimal NewTotal);

public sealed record RiskExposureClosedEventSchema(
    Guid AggregateId);

/// <summary>
/// Phase 6 Final Patch — on-wire shape of
/// <c>Whycespace.Domain.DecisionSystem.Risk.Exposure.ExposureBreachedEvent</c>.
/// Primitives only so the deserialization path stays symmetric with every
/// other domain event in the economic schema catalog. Consumed by
/// <c>RiskExposureEnforcementHandler</c> via the Kafka event fabric to
/// dispatch a <c>DetectViolationCommand</c> into the enforcement pipeline.
/// </summary>
public sealed record RiskExposureBreachedEventSchema(
    Guid AggregateId,
    decimal TotalExposure,
    decimal Threshold,
    string Currency,
    DateTimeOffset DetectedAt);
