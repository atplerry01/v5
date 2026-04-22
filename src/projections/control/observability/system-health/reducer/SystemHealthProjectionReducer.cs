using Whycespace.Shared.Contracts.Control.Observability.SystemHealth;
using Whycespace.Shared.Contracts.Events.Control.Observability.SystemHealth;

namespace Whycespace.Projections.Control.Observability.SystemHealth.Reducer;

public static class SystemHealthProjectionReducer
{
    public static SystemHealthReadModel Apply(SystemHealthReadModel state, SystemHealthEvaluatedEventSchema e) =>
        state with
        {
            HealthId         = e.AggregateId,
            ComponentName    = e.ComponentName,
            Status           = e.Status,
            LastEvaluatedAt  = e.EvaluatedAt
        };

    public static SystemHealthReadModel Apply(SystemHealthReadModel state, SystemHealthDegradedEventSchema e) =>
        state with
        {
            Status            = e.NewStatus,
            LastEvaluatedAt   = e.OccurredAt,
            DegradationReason = e.Reason
        };

    public static SystemHealthReadModel Apply(SystemHealthReadModel state, SystemHealthRestoredEventSchema e) =>
        state with
        {
            Status            = "Healthy",
            LastEvaluatedAt   = e.RestoredAt,
            DegradationReason = null
        };
}
