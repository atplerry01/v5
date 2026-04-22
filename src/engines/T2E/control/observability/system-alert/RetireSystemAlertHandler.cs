using Whycespace.Domain.ControlSystem.Observability.SystemAlert;
using Whycespace.Shared.Contracts.Control.Observability.SystemAlert;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Control.Observability.SystemAlert;

public sealed class RetireSystemAlertHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not RetireSystemAlertCommand)
            return;

        var aggregate = (SystemAlertAggregate)await context.LoadAggregateAsync(typeof(SystemAlertAggregate));
        aggregate.Retire();
        context.EmitEvents(aggregate.DomainEvents);
    }
}
