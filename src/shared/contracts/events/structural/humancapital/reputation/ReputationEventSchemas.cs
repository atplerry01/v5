namespace Whycespace.Shared.Contracts.Events.Structural.Humancapital.Reputation;

public sealed record ReputationCreatedEventSchema(
    Guid AggregateId,
    string Name,
    string Kind);
