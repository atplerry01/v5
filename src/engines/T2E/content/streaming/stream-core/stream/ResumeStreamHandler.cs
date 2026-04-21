using Whycespace.Domain.ContentSystem.Streaming.StreamCore.Stream;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Streaming.StreamCore.Stream;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Streaming.StreamCore.Stream;

public sealed class ResumeStreamHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ResumeStreamCommand cmd) return;
        var aggregate = (StreamAggregate)await context.LoadAggregateAsync(typeof(StreamAggregate));
        aggregate.Resume(new Timestamp(cmd.ResumedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
