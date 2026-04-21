using Whycespace.Domain.ContentSystem.Streaming.StreamCore.Stream;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Streaming.StreamCore.Stream;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Streaming.StreamCore.Stream;

public sealed class ActivateStreamHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ActivateStreamCommand cmd) return;
        var aggregate = (StreamAggregate)await context.LoadAggregateAsync(typeof(StreamAggregate));
        aggregate.Activate(new Timestamp(cmd.ActivatedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
