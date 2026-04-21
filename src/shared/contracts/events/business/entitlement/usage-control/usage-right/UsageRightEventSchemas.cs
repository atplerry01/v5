namespace Whycespace.Shared.Contracts.Events.Business.Entitlement.UsageControl.UsageRight;

public sealed record UsageRightCreatedEventSchema(
    Guid AggregateId,
    Guid SubjectId,
    Guid ReferenceId,
    int TotalUnits);

public sealed record UsageRightUsedEventSchema(
    Guid AggregateId,
    Guid RecordId,
    int UnitsUsed);

public sealed record UsageRightConsumedEventSchema(Guid AggregateId);
