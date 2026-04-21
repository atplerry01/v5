using Whycespace.Domain.ContentSystem.Media.CoreObject.Asset;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Media.CoreObject.Asset;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Media.CoreObject.Asset;

public sealed class AssignAssetKindHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not AssignAssetKindCommand cmd) return;
        var aggregate = (AssetAggregate)await context.LoadAggregateAsync(typeof(AssetAggregate));
        aggregate.AssignKind(Enum.Parse<AssetKind>(cmd.NewKind), new Timestamp(cmd.AssignedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
