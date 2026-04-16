using Whycespace.Shared.Contracts.Economic.Routing.Execution;
using Whycespace.Shared.Contracts.Events.Economic.Routing.Execution;

namespace Whycespace.Projections.Economic.Routing.Execution.Reducer;

public static class RoutingExecutionProjectionReducer
{
    public static RoutingExecutionReadModel Apply(RoutingExecutionReadModel state, ExecutionStartedEventSchema e) =>
        state with
        {
            ExecutionId = e.AggregateId,
            PathId = e.PathId,
            Status = "Started",
            StartedAt = e.StartedAt
        };

    public static RoutingExecutionReadModel Apply(RoutingExecutionReadModel state, ExecutionCompletedEventSchema e) =>
        state with
        {
            ExecutionId = e.AggregateId,
            Status = "Completed",
            TerminalAt = e.CompletedAt
        };

    public static RoutingExecutionReadModel Apply(RoutingExecutionReadModel state, ExecutionFailedEventSchema e) =>
        state with
        {
            ExecutionId = e.AggregateId,
            Status = "Failed",
            TerminalAt = e.FailedAt,
            TerminalReason = e.Reason
        };

    public static RoutingExecutionReadModel Apply(RoutingExecutionReadModel state, ExecutionAbortedEventSchema e) =>
        state with
        {
            ExecutionId = e.AggregateId,
            Status = "Aborted",
            TerminalAt = e.AbortedAt,
            TerminalReason = e.Reason
        };
}
