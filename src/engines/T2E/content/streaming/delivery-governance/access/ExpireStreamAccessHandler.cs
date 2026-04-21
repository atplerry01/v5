using Whycespace.Domain.ContentSystem.Streaming.DeliveryGovernance.Access;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Streaming.DeliveryGovernance.Access;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Streaming.DeliveryGovernance.Access;

public sealed class ExpireStreamAccessHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ExpireStreamAccessCommand cmd) return;
        var aggregate = (StreamAccessAggregate)await context.LoadAggregateAsync(typeof(StreamAccessAggregate));
        aggregate.Expire(new Timestamp(cmd.ExpiredAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
