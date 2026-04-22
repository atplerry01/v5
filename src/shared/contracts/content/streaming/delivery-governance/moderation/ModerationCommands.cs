using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Content.Streaming.DeliveryGovernance.Moderation;

public sealed record FlagStreamCommand(
    Guid ModerationId,
    Guid TargetId,
    string FlagReason) : IHasAggregateId
{
    public Guid AggregateId => ModerationId;
}

public sealed record AssignModerationCommand(
    Guid ModerationId,
    Guid ModeratorId,
    DateTimeOffset AssignedAt) : IHasAggregateId
{
    public Guid AggregateId => ModerationId;
}

public sealed record DecideModerationCommand(
    Guid ModerationId,
    string Decision,
    string Rationale,
    DateTimeOffset DecidedAt) : IHasAggregateId
{
    public Guid AggregateId => ModerationId;
}

public sealed record OverturnModerationCommand(
    Guid ModerationId,
    string Rationale,
    DateTimeOffset OverturnedAt) : IHasAggregateId
{
    public Guid AggregateId => ModerationId;
}
