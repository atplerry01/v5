namespace Whycespace.Shared.Contracts.Events.Structural.Humancapital.Stewardship;

public sealed record StewardshipCreatedEventSchema(
    Guid AggregateId,
    string Name,
    string Kind);
