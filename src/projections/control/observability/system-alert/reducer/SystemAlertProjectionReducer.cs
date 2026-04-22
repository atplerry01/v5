using Whycespace.Shared.Contracts.Control.Observability.SystemAlert;
using Whycespace.Shared.Contracts.Events.Control.Observability.SystemAlert;

namespace Whycespace.Projections.Control.Observability.SystemAlert.Reducer;

public static class SystemAlertProjectionReducer
{
    public static SystemAlertReadModel Apply(SystemAlertReadModel state, SystemAlertDefinedEventSchema e) =>
        state with
        {
            AlertId              = e.AggregateId,
            Name                 = e.Name,
            MetricDefinitionId   = e.MetricDefinitionId,
            ConditionExpression  = e.ConditionExpression,
            Severity             = e.Severity,
            Status               = "Active"
        };

    public static SystemAlertReadModel Apply(SystemAlertReadModel state, SystemAlertRetiredEventSchema e) =>
        state with { Status = "Retired" };
}
