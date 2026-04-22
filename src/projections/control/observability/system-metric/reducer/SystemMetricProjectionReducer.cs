using Whycespace.Shared.Contracts.Control.Observability.SystemMetric;
using Whycespace.Shared.Contracts.Events.Control.Observability.SystemMetric;

namespace Whycespace.Projections.Control.Observability.SystemMetric.Reducer;

public static class SystemMetricProjectionReducer
{
    public static SystemMetricReadModel Apply(SystemMetricReadModel state, SystemMetricDefinedEventSchema e) =>
        state with
        {
            MetricId     = e.AggregateId,
            Name         = e.Name,
            Type         = e.Type,
            Unit         = e.Unit,
            LabelNames   = e.LabelNames,
            IsDeprecated = false
        };

    public static SystemMetricReadModel Apply(SystemMetricReadModel state, SystemMetricDeprecatedEventSchema e) =>
        state with { IsDeprecated = true };
}
