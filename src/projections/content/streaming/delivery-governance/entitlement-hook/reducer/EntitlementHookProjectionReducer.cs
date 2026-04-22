using Whycespace.Shared.Contracts.Content.Streaming.DeliveryGovernance.EntitlementHook;
using Whycespace.Shared.Contracts.Events.Content.Streaming.DeliveryGovernance.EntitlementHook;

namespace Whycespace.Projections.Content.Streaming.DeliveryGovernance.EntitlementHook.Reducer;

public static class EntitlementHookProjectionReducer
{
    public static EntitlementHookReadModel Apply(EntitlementHookReadModel state, EntitlementHookRegisteredEventSchema e) =>
        state with
        {
            HookId = e.AggregateId,
            TargetId = e.TargetId,
            SourceSystem = e.SourceSystem,
            Status = "Unknown",
            FailureReason = null,
            LastCheckedAt = null,
            RegisteredAt = e.RegisteredAt,
            LastModifiedAt = e.RegisteredAt
        };

    public static EntitlementHookReadModel Apply(EntitlementHookReadModel state, EntitlementQueriedEventSchema e) =>
        state with
        {
            HookId = e.AggregateId,
            Status = e.Result,
            FailureReason = null,
            LastCheckedAt = e.QueriedAt,
            LastModifiedAt = e.QueriedAt
        };

    public static EntitlementHookReadModel Apply(EntitlementHookReadModel state, EntitlementRefreshedEventSchema e) =>
        state with
        {
            HookId = e.AggregateId,
            Status = e.Result,
            FailureReason = null,
            LastCheckedAt = e.RefreshedAt,
            LastModifiedAt = e.RefreshedAt
        };

    public static EntitlementHookReadModel Apply(EntitlementHookReadModel state, EntitlementInvalidatedEventSchema e) =>
        state with
        {
            HookId = e.AggregateId,
            Status = "Unknown",
            LastModifiedAt = e.InvalidatedAt
        };

    public static EntitlementHookReadModel Apply(EntitlementHookReadModel state, EntitlementFailureRecordedEventSchema e) =>
        state with
        {
            HookId = e.AggregateId,
            Status = "Error",
            FailureReason = e.Reason,
            LastCheckedAt = e.FailedAt,
            LastModifiedAt = e.FailedAt
        };
}
