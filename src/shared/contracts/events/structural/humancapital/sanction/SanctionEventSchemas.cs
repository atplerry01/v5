namespace Whycespace.Shared.Contracts.Events.Structural.Humancapital.Sanction;

public sealed record SanctionCreatedEventSchema(
    Guid AggregateId,
    string Name,
    string Kind);
