using Whycespace.Domain.ControlSystem.Enforcement.Sanction;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Economic.Enforcement.Sanction;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Enforcement.Sanction;

/// <summary>
/// Phase 7 T7.11 — terminal natural expiry. Idempotent under replay:
/// a second dispatch against an already-Expired aggregate emits no
/// events (no-op) rather than throwing, so at-least-once delivery from
/// any future scheduler can't produce a partial clear.
/// </summary>
public sealed class ExpireSanctionHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ExpireSanctionCommand cmd)
            return;

        var aggregate = (SanctionAggregate)await context.LoadAggregateAsync(typeof(SanctionAggregate));

        if (aggregate.Status == SanctionStatus.Expired)
            return;

        aggregate.Expire(new Timestamp(cmd.ExpiredAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
