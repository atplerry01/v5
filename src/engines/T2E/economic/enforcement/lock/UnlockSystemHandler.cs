using Whycespace.Domain.ControlSystem.Enforcement.Lock;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Economic.Enforcement.Lock;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Enforcement.Lock;

public sealed class UnlockSystemHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not UnlockSystemCommand cmd)
            return;

        var aggregate = (LockAggregate)await context.LoadAggregateAsync(typeof(LockAggregate));
        aggregate.Unlock(new Reason(cmd.UnlockReason), new Timestamp(cmd.UnlockedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
