using Whycespace.Domain.ContentSystem.Media.LifecycleChange.Version;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Media.LifecycleChange.Version;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Media.LifecycleChange.Version;

public sealed class CreateMediaVersionHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CreateMediaVersionCommand cmd) return Task.CompletedTask;
        MediaVersionId? previousId = cmd.PreviousVersionId.HasValue ? new MediaVersionId(cmd.PreviousVersionId.Value) : null;
        var aggregate = MediaVersionAggregate.Create(
            new MediaVersionId(cmd.VersionId),
            new MediaAssetRef(cmd.AssetRef),
            new MediaVersionNumber(cmd.VersionMajor, cmd.VersionMinor),
            new MediaFileRef(cmd.FileRef),
            previousId,
            new Timestamp(cmd.CreatedAt));
        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
