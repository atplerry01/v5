using Whycespace.Domain.ControlSystem.Observability.SystemSignal;
using Whycespace.Shared.Contracts.Control.Observability.SystemSignal;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Control.Observability.SystemSignal;

public sealed class DeprecateSystemSignalHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not DeprecateSystemSignalCommand)
            return;

        var aggregate = (SystemSignalAggregate)await context.LoadAggregateAsync(typeof(SystemSignalAggregate));
        aggregate.Deprecate();
        context.EmitEvents(aggregate.DomainEvents);
    }
}
