namespace Whycespace.Shared.Contracts.Events.Structural.Humancapital.Eligibility;

public sealed record EligibilityCreatedEventSchema(
    Guid AggregateId,
    string Name,
    string Kind);
