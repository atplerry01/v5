using Whycespace.Domain.ContentSystem.Media.LifecycleChange.Version;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Media.LifecycleChange.Version;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Media.LifecycleChange.Version;

public sealed class WithdrawMediaVersionHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not WithdrawMediaVersionCommand cmd) return;
        var aggregate = (MediaVersionAggregate)await context.LoadAggregateAsync(typeof(MediaVersionAggregate));
        aggregate.Withdraw(cmd.Reason, new Timestamp(cmd.WithdrawnAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
