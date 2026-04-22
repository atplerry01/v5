using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Content.Streaming.DeliveryGovernance.EntitlementHook;

public sealed record RegisterEntitlementHookCommand(
    Guid HookId,
    Guid TargetId,
    string SourceSystem) : IHasAggregateId
{
    public Guid AggregateId => HookId;
}

public sealed record RecordEntitlementQueryCommand(
    Guid HookId,
    string Result,
    DateTimeOffset QueriedAt) : IHasAggregateId
{
    public Guid AggregateId => HookId;
}

public sealed record RefreshEntitlementCommand(
    Guid HookId,
    string Result,
    DateTimeOffset RefreshedAt) : IHasAggregateId
{
    public Guid AggregateId => HookId;
}

public sealed record InvalidateEntitlementCommand(
    Guid HookId,
    DateTimeOffset InvalidatedAt) : IHasAggregateId
{
    public Guid AggregateId => HookId;
}

public sealed record RecordEntitlementFailureCommand(
    Guid HookId,
    string Reason,
    DateTimeOffset FailedAt) : IHasAggregateId
{
    public Guid AggregateId => HookId;
}
