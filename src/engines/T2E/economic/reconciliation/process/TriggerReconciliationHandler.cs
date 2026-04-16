using Whycespace.Domain.EconomicSystem.Reconciliation.Process;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Economic.Reconciliation.Process;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Reconciliation.Process;

public sealed class TriggerReconciliationHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not TriggerReconciliationCommand cmd)
            return Task.CompletedTask;

        var aggregate = ProcessAggregate.Trigger(
            new ProcessId(cmd.ProcessId),
            new SourceReference(cmd.LedgerReference),
            new SourceReference(cmd.ObservedReference),
            new Timestamp(cmd.TriggeredAt));

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
