using Whycespace.Domain.ContentSystem.Streaming.StreamCore.Manifest;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Streaming.StreamCore.Manifest;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Streaming.StreamCore.Manifest;

public sealed class CreateManifestHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CreateManifestCommand cmd) return Task.CompletedTask;
        var aggregate = ManifestAggregate.Create(
            new ManifestId(cmd.ManifestId),
            new ManifestSourceRef(cmd.SourceId, Enum.Parse<ManifestSourceKind>(cmd.SourceKind)),
            new Timestamp(cmd.CreatedAt));
        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
