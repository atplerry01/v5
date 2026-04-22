using Whycespace.Domain.ControlSystem.SystemReconciliation.ConsistencyCheck;
using Whycespace.Shared.Contracts.Control.SystemReconciliation.ConsistencyCheck;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Control.SystemReconciliation.ConsistencyCheck;

public sealed class InitiateConsistencyCheckHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not InitiateConsistencyCheckCommand cmd)
            return Task.CompletedTask;

        var aggregate = ConsistencyCheckAggregate.Initiate(
            new ConsistencyCheckId(cmd.CheckId.ToString("N").PadRight(64, '0')),
            cmd.ScopeTarget,
            cmd.InitiatedAt);

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
