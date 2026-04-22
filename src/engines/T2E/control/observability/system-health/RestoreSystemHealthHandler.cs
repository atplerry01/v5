using Whycespace.Domain.ControlSystem.Observability.SystemHealth;
using Whycespace.Shared.Contracts.Control.Observability.SystemHealth;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Control.Observability.SystemHealth;

public sealed class RestoreSystemHealthHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not RestoreSystemHealthCommand cmd)
            return;

        var aggregate = (SystemHealthAggregate)await context.LoadAggregateAsync(typeof(SystemHealthAggregate));
        aggregate.Restore(cmd.RestoredAt);
        context.EmitEvents(aggregate.DomainEvents);
    }
}
