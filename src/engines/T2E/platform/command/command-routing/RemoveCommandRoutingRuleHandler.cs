using Whycespace.Domain.PlatformSystem.Command.CommandRouting;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Platform.Command.CommandRouting;

namespace Whycespace.Engines.T2E.Platform.Command.CommandRouting;

public sealed class RemoveCommandRoutingRuleHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not RemoveCommandRoutingRuleCommand cmd)
            return;

        var aggregate = (CommandRoutingRuleAggregate)await context.LoadAggregateAsync(typeof(CommandRoutingRuleAggregate));
        aggregate.Remove(new Timestamp(cmd.RemovedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
