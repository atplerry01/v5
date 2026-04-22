using Whycespace.Domain.ControlSystem.SystemReconciliation.ReconciliationRun;
using Whycespace.Shared.Contracts.Control.SystemReconciliation.ReconciliationRun;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Control.SystemReconciliation.ReconciliationRun;

public sealed class ScheduleReconciliationRunHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ScheduleReconciliationRunCommand cmd)
            return Task.CompletedTask;

        var aggregate = ReconciliationRunAggregate.Schedule(
            new ReconciliationRunId(cmd.RunId.ToString("N").PadRight(64, '0')),
            cmd.Scope);

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
