using Whycespace.Domain.ContentSystem.Streaming.DeliveryGovernance.Moderation;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Streaming.DeliveryGovernance.Moderation;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Streaming.DeliveryGovernance.Moderation;

public sealed class DecideModerationHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not DecideModerationCommand cmd) return;
        var aggregate = (ModerationAggregate)await context.LoadAggregateAsync(typeof(ModerationAggregate));
        aggregate.Decide(Enum.Parse<ModerationDecision>(cmd.Decision), cmd.Rationale, new Timestamp(cmd.DecidedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
