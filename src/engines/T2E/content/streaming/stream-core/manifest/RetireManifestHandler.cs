using Whycespace.Domain.ContentSystem.Streaming.StreamCore.Manifest;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Streaming.StreamCore.Manifest;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Streaming.StreamCore.Manifest;

public sealed class RetireManifestHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not RetireManifestCommand cmd) return;
        var aggregate = (ManifestAggregate)await context.LoadAggregateAsync(typeof(ManifestAggregate));
        aggregate.Retire(cmd.Reason, new Timestamp(cmd.RetiredAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
