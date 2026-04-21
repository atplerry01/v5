using Whycespace.Domain.ContentSystem.Streaming.PlaybackConsumption.Session;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Streaming.PlaybackConsumption.Session;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Streaming.PlaybackConsumption.Session;

public sealed class ResumeSessionHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ResumeSessionCommand cmd) return;
        var aggregate = (SessionAggregate)await context.LoadAggregateAsync(typeof(SessionAggregate));
        aggregate.Resume(new Timestamp(cmd.ResumedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
