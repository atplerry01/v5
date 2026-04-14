using Whycespace.Domain.EconomicSystem.Capital.Asset;
using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Economic.Capital.Asset;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Capital.Asset;

public sealed class RevalueAssetHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not RevalueAssetCommand cmd)
            return;

        var aggregate = (AssetAggregate)await context.LoadAggregateAsync(typeof(AssetAggregate));
        aggregate.Revalue(new Amount(cmd.NewValue), new Timestamp(cmd.ValuedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
