namespace Whycespace.Shared.Contracts.Events.Structural.Humancapital.Operator;

public sealed record OperatorCreatedEventSchema(
    Guid AggregateId,
    string Name,
    string Kind);
