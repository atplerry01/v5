using Whycespace.Domain.ContentSystem.Media.Asset;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Media.Asset;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Media.Asset;

public sealed class RegisterMediaAssetHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not RegisterMediaAssetCommand cmd)
            return Task.CompletedTask;

        var aggregate = MediaAssetAggregate.Register(
            new EventId(cmd.CommandId),
            new AggregateId(cmd.AggregateId),
            new CorrelationId(cmd.CorrelationId),
            new CausationId(cmd.CausationId),
            MediaAssetId.From(cmd.AssetId),
            cmd.OwnerRef,
            (MediaType)cmd.MediaType,
            MediaTitle.Create(cmd.Title),
            MediaDescription.Create(cmd.Description),
            ContentDigest.Create(cmd.ContentDigest),
            StorageLocation.Create(cmd.StorageUri, cmd.StorageSizeBytes),
            new Timestamp(cmd.OccurredAt));

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
