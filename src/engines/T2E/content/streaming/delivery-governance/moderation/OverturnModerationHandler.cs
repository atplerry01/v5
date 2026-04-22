using Whycespace.Domain.ContentSystem.Streaming.DeliveryGovernance.Moderation;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Streaming.DeliveryGovernance.Moderation;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Streaming.DeliveryGovernance.Moderation;

public sealed class OverturnModerationHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not OverturnModerationCommand cmd) return;
        var aggregate = (ModerationAggregate)await context.LoadAggregateAsync(typeof(ModerationAggregate));
        aggregate.Overturn(cmd.Rationale, new Timestamp(cmd.OverturnedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
