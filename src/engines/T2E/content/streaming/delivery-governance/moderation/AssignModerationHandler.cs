using Whycespace.Domain.ContentSystem.Streaming.DeliveryGovernance.Moderation;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Streaming.DeliveryGovernance.Moderation;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Streaming.DeliveryGovernance.Moderation;

public sealed class AssignModerationHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not AssignModerationCommand cmd) return;
        var aggregate = (ModerationAggregate)await context.LoadAggregateAsync(typeof(ModerationAggregate));
        aggregate.Assign(new ModeratorRef(cmd.ModeratorId), new Timestamp(cmd.AssignedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
