using Whycespace.Domain.OperationalSystem.Routing.Path;

namespace Whycespace.Domain.OperationalSystem.Routing.Execution;

public sealed class ExecutionService
{
    /// <summary>
    /// Determines whether an execution may be started against a given routing path.
    /// Only active paths are eligible as execution targets.
    /// </summary>
    public bool CanStartOn(RoutingPathAggregate path)
    {
        return path.Status == RoutingPathStatus.Active;
    }

    /// <summary>
    /// Determines whether a terminal transition (Complete/Fail/Abort) is permitted
    /// for an execution based on its current status.
    /// </summary>
    public bool CanTerminate(ExecutionAggregate execution)
    {
        return execution.Status == ExecutionStatus.Started;
    }
}
