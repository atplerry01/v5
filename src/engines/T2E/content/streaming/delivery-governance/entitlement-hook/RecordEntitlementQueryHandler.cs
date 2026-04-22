using Whycespace.Domain.ContentSystem.Streaming.DeliveryGovernance.EntitlementHook;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Streaming.DeliveryGovernance.EntitlementHook;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Streaming.DeliveryGovernance.EntitlementHook;

public sealed class RecordEntitlementQueryHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not RecordEntitlementQueryCommand cmd) return;
        var aggregate = (EntitlementHookAggregate)await context.LoadAggregateAsync(typeof(EntitlementHookAggregate));
        aggregate.RecordQuery(Enum.Parse<EntitlementStatus>(cmd.Result), new Timestamp(cmd.QueriedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
