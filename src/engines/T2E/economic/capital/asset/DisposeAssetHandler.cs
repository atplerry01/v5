using Whycespace.Domain.EconomicSystem.Capital.Asset;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Economic.Capital.Asset;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Capital.Asset;

public sealed class DisposeAssetHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not DisposeAssetCommand cmd)
            return;

        var aggregate = (AssetAggregate)await context.LoadAggregateAsync(typeof(AssetAggregate));
        aggregate.Dispose(new Timestamp(cmd.DisposedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
