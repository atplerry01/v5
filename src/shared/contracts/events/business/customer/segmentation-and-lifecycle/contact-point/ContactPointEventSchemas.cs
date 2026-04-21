namespace Whycespace.Shared.Contracts.Events.Business.Customer.SegmentationAndLifecycle.ContactPoint;

public sealed record ContactPointCreatedEventSchema(
    Guid AggregateId,
    Guid CustomerId,
    string Kind,
    string Value);

public sealed record ContactPointUpdatedEventSchema(
    Guid AggregateId,
    string Value);

public sealed record ContactPointActivatedEventSchema(Guid AggregateId);

public sealed record ContactPointPreferredSetEventSchema(
    Guid AggregateId,
    bool IsPreferred);

public sealed record ContactPointArchivedEventSchema(Guid AggregateId);
