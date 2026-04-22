using Whycespace.Domain.ContentSystem.Streaming.DeliveryGovernance.EntitlementHook;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Streaming.DeliveryGovernance.EntitlementHook;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Streaming.DeliveryGovernance.EntitlementHook;

public sealed class RecordEntitlementFailureHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not RecordEntitlementFailureCommand cmd) return;
        var aggregate = (EntitlementHookAggregate)await context.LoadAggregateAsync(typeof(EntitlementHookAggregate));
        aggregate.RecordFailure(cmd.Reason, new Timestamp(cmd.FailedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
