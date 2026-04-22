using Whycespace.Domain.PlatformSystem.Routing.DispatchRule;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Platform.Routing.DispatchRule;

namespace Whycespace.Engines.T2E.Platform.Routing.DispatchRule;

public sealed class DeactivateDispatchRuleHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not DeactivateDispatchRuleCommand cmd)
            return;

        var aggregate = (DispatchRuleAggregate)await context.LoadAggregateAsync(typeof(DispatchRuleAggregate));
        aggregate.Deactivate(new Timestamp(cmd.DeactivatedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
