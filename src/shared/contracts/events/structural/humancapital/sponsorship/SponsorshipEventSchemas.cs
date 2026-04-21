namespace Whycespace.Shared.Contracts.Events.Structural.Humancapital.Sponsorship;

public sealed record SponsorshipCreatedEventSchema(
    Guid AggregateId,
    string Name,
    string Kind);
