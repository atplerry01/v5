using Whycespace.Domain.ContentSystem.Streaming.DeliveryGovernance.EntitlementHook;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Streaming.DeliveryGovernance.EntitlementHook;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Streaming.DeliveryGovernance.EntitlementHook;

public sealed class InvalidateEntitlementHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not InvalidateEntitlementCommand cmd) return;
        var aggregate = (EntitlementHookAggregate)await context.LoadAggregateAsync(typeof(EntitlementHookAggregate));
        aggregate.Invalidate(new Timestamp(cmd.InvalidatedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
