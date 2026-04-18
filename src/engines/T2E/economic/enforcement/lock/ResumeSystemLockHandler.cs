using Whycespace.Domain.EconomicSystem.Enforcement.Lock;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Economic.Enforcement.Lock;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Enforcement.Lock;

/// <summary>
/// Phase 7 T7.8 — resumes a Suspended lock back to Locked. The aggregate
/// has retained its original Scope/Reason/Cause/ExpiresAt throughout
/// suspension, so the transition is a pure state restore — no field
/// is recomputed.
/// </summary>
public sealed class ResumeSystemLockHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ResumeSystemLockCommand cmd)
            return;

        var aggregate = (LockAggregate)await context.LoadAggregateAsync(typeof(LockAggregate));
        aggregate.Resume(new Timestamp(cmd.ResumedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
