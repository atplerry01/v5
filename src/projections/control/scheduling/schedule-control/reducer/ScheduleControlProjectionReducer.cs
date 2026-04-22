using Whycespace.Shared.Contracts.Control.Scheduling.ScheduleControl;
using Whycespace.Shared.Contracts.Events.Control.Scheduling.ScheduleControl;

namespace Whycespace.Projections.Control.Scheduling.ScheduleControl.Reducer;

public static class ScheduleControlProjectionReducer
{
    public static ScheduleControlReadModel Apply(ScheduleControlReadModel state, ScheduleControlDefinedEventSchema e) =>
        state with
        {
            ScheduleId        = e.AggregateId,
            JobDefinitionId   = e.JobDefinitionId,
            TriggerExpression = e.TriggerExpression,
            Status            = "Active"
        };

    public static ScheduleControlReadModel Apply(ScheduleControlReadModel state, ScheduleControlSuspendedEventSchema e) =>
        state with { Status = "Suspended" };

    public static ScheduleControlReadModel Apply(ScheduleControlReadModel state, ScheduleControlResumedEventSchema e) =>
        state with { Status = "Active" };

    public static ScheduleControlReadModel Apply(ScheduleControlReadModel state, ScheduleControlRetiredEventSchema e) =>
        state with { Status = "Retired" };
}
