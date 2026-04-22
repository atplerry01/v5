using Whycespace.Shared.Contracts.Control.Scheduling.ExecutionControl;
using Whycespace.Shared.Contracts.Events.Control.Scheduling.ExecutionControl;

namespace Whycespace.Projections.Control.Scheduling.ExecutionControl.Reducer;

public static class ExecutionControlProjectionReducer
{
    public static ExecutionControlReadModel Apply(ExecutionControlReadModel state, ExecutionControlSignalIssuedEventSchema e) =>
        state with
        {
            ControlId     = e.AggregateId,
            JobInstanceId = e.JobInstanceId,
            Signal        = e.Signal,
            ActorId       = e.ActorId,
            IssuedAt      = e.IssuedAt
        };

    public static ExecutionControlReadModel Apply(ExecutionControlReadModel state, ExecutionControlSignalOutcomeRecordedEventSchema e) =>
        state with { Outcome = e.Outcome };
}
