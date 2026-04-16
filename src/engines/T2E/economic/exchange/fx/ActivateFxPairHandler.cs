using Whycespace.Domain.EconomicSystem.Exchange.Fx;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Economic.Exchange.Fx;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Exchange.Fx;

public sealed class ActivateFxPairHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ActivateFxPairCommand cmd)
            return;

        var aggregate = (FxAggregate)await context.LoadAggregateAsync(typeof(FxAggregate));
        aggregate.Activate(new Timestamp(cmd.ActivatedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
