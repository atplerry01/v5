namespace Whycespace.Shared.Contracts.Events.Structural.Humancapital.Workforce;

public sealed record WorkforceCreatedEventSchema(
    Guid AggregateId,
    string Name,
    string Kind);
