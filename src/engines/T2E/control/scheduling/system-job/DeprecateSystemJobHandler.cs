using Whycespace.Domain.ControlSystem.Scheduling.SystemJob;
using Whycespace.Shared.Contracts.Control.Scheduling.SystemJob;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Control.Scheduling.SystemJob;

public sealed class DeprecateSystemJobHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not DeprecateSystemJobCommand)
            return;

        var aggregate = (SystemJobAggregate)await context.LoadAggregateAsync(typeof(SystemJobAggregate));
        aggregate.Deprecate();
        context.EmitEvents(aggregate.DomainEvents);
    }
}
