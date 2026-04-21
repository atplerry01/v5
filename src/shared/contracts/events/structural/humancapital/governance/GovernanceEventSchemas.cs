namespace Whycespace.Shared.Contracts.Events.Structural.Humancapital.Governance;

public sealed record GovernanceCreatedEventSchema(
    Guid AggregateId,
    string Name,
    string Kind);
