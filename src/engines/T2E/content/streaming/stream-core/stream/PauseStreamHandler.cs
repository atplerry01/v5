using Whycespace.Domain.ContentSystem.Streaming.StreamCore.Stream;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Streaming.StreamCore.Stream;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Streaming.StreamCore.Stream;

public sealed class PauseStreamHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not PauseStreamCommand cmd) return;
        var aggregate = (StreamAggregate)await context.LoadAggregateAsync(typeof(StreamAggregate));
        aggregate.Pause(new Timestamp(cmd.PausedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
