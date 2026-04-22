using Whycespace.Shared.Contracts.Control.Scheduling.SystemJob;
using Whycespace.Shared.Contracts.Events.Control.Scheduling.SystemJob;

namespace Whycespace.Projections.Control.Scheduling.SystemJob.Reducer;

public static class SystemJobProjectionReducer
{
    public static SystemJobReadModel Apply(SystemJobReadModel state, SystemJobDefinedEventSchema e) =>
        state with
        {
            JobId        = e.AggregateId,
            Name         = e.Name,
            Category     = e.Category,
            Timeout      = e.Timeout,
            IsDeprecated = false
        };

    public static SystemJobReadModel Apply(SystemJobReadModel state, SystemJobDeprecatedEventSchema e) =>
        state with { IsDeprecated = true };
}
