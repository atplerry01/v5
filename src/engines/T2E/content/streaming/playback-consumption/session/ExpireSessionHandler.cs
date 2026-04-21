using Whycespace.Domain.ContentSystem.Streaming.PlaybackConsumption.Session;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Streaming.PlaybackConsumption.Session;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Streaming.PlaybackConsumption.Session;

public sealed class ExpireSessionHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ExpireSessionCommand cmd) return;
        var aggregate = (SessionAggregate)await context.LoadAggregateAsync(typeof(SessionAggregate));
        aggregate.Expire(new Timestamp(cmd.ExpiredAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
