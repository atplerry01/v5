namespace Whycespace.Shared.Contracts.Events.Structural.Humancapital.Performance;

public sealed record PerformanceCreatedEventSchema(
    Guid AggregateId,
    string Name,
    string Kind);
