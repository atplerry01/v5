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
