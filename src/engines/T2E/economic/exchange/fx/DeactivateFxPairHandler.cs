using Whycespace.Domain.EconomicSystem.Exchange.Fx;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Economic.Exchange.Fx;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Exchange.Fx;

public sealed class DeactivateFxPairHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not DeactivateFxPairCommand cmd)
            return;

        var aggregate = (FxAggregate)await context.LoadAggregateAsync(typeof(FxAggregate));
        aggregate.Deactivate(new Timestamp(cmd.DeactivatedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
