using Whycespace.Domain.EconomicSystem.Enforcement.Lock;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Economic.Enforcement.Lock;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Enforcement.Lock;

/// <summary>
/// Phase 7 T7.9 — marks a Locked lock as naturally Expired. The aggregate
/// rejects expiry on any state other than Locked, so a suspended lock
/// must be Resumed before expiry. The actual scheduling that dispatches
/// this command at <c>ExpiresAt</c> is out of scope for this batch —
/// this handler serves the state transition once the command arrives.
/// </summary>
public sealed class ExpireSystemLockHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ExpireSystemLockCommand cmd)
            return;

        var aggregate = (LockAggregate)await context.LoadAggregateAsync(typeof(LockAggregate));
        aggregate.Expire(new Timestamp(cmd.ExpiredAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
