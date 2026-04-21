using Whycespace.Domain.ContentSystem.Streaming.StreamCore.Stream;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Streaming.StreamCore.Stream;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Streaming.StreamCore.Stream;

public sealed class EndStreamHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not EndStreamCommand cmd) return;
        var aggregate = (StreamAggregate)await context.LoadAggregateAsync(typeof(StreamAggregate));
        aggregate.End(new Timestamp(cmd.EndedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
