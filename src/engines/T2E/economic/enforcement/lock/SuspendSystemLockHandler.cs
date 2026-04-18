using Whycespace.Domain.EconomicSystem.Enforcement.Lock;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Economic.Enforcement.Lock;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Enforcement.Lock;

/// <summary>
/// Phase 7 T7.8 — suspends a Locked lock for a bounded cause (typically
/// a compensation flow against the same subject). The underlying Locked
/// state is preserved on the aggregate; Resume restores it without
/// inventing any state. Expire is not valid from Suspended — a paused
/// timer doesn't count down.
/// </summary>
public sealed class SuspendSystemLockHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not SuspendSystemLockCommand cmd)
            return;

        if (!Enum.TryParse<EnforcementCauseKind>(cmd.SuspensionCause.Kind, ignoreCase: true, out var kind))
            throw new InvalidOperationException($"Unknown EnforcementCauseKind: '{cmd.SuspensionCause.Kind}'.");

        var suspensionCause = new EnforcementCause(
            kind,
            cmd.SuspensionCause.CauseReferenceId,
            cmd.SuspensionCause.Detail);

        var aggregate = (LockAggregate)await context.LoadAggregateAsync(typeof(LockAggregate));
        aggregate.Suspend(suspensionCause, new Timestamp(cmd.SuspendedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
