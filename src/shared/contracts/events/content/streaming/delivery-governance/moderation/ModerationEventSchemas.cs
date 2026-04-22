namespace Whycespace.Shared.Contracts.Events.Content.Streaming.DeliveryGovernance.Moderation;

public sealed record StreamFlaggedEventSchema(
    Guid AggregateId,
    Guid TargetId,
    string FlagReason,
    DateTimeOffset FlaggedAt);

public sealed record ModerationAssignedEventSchema(
    Guid AggregateId,
    Guid ModeratorId,
    DateTimeOffset AssignedAt);

public sealed record ModerationDecidedEventSchema(
    Guid AggregateId,
    string Decision,
    string Rationale,
    DateTimeOffset DecidedAt);

public sealed record ModerationOverturnedEventSchema(
    Guid AggregateId,
    string Rationale,
    DateTimeOffset OverturnedAt);
