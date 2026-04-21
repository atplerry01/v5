using Whycespace.Domain.ContentSystem.Streaming.DeliveryGovernance.Access;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Streaming.DeliveryGovernance.Access;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Streaming.DeliveryGovernance.Access;

public sealed class RevokeStreamAccessHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not RevokeStreamAccessCommand cmd) return;
        var aggregate = (StreamAccessAggregate)await context.LoadAggregateAsync(typeof(StreamAccessAggregate));
        aggregate.Revoke(cmd.Reason, new Timestamp(cmd.RevokedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
