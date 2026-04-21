namespace Whycespace.Shared.Contracts.Events.Structural.Humancapital.Incentive;

public sealed record IncentiveCreatedEventSchema(
    Guid AggregateId,
    string Name,
    string Kind);
