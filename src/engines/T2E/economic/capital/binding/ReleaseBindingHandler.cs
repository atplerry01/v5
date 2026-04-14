using Whycespace.Domain.EconomicSystem.Capital.Binding;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Economic.Capital.Binding;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Capital.Binding;

public sealed class ReleaseBindingHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ReleaseBindingCommand cmd)
            return;

        var aggregate = (BindingAggregate)await context.LoadAggregateAsync(typeof(BindingAggregate));
        aggregate.Release(new Timestamp(cmd.ReleasedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
