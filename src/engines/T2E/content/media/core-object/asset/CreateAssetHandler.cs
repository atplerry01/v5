using Whycespace.Domain.ContentSystem.Media.CoreObject.Asset;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Media.CoreObject.Asset;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Media.CoreObject.Asset;

public sealed class CreateAssetHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CreateAssetCommand cmd) return Task.CompletedTask;
        var aggregate = AssetAggregate.Create(
            new AssetId(cmd.AssetId),
            new AssetTitle(cmd.Title),
            Enum.Parse<AssetClassification>(cmd.Classification),
            new Timestamp(cmd.CreatedAt));
        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
