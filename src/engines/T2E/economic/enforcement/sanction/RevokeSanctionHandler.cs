using Whycespace.Domain.ControlSystem.Enforcement.Sanction;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Economic.Enforcement.Sanction;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Enforcement.Sanction;

/// <summary>
/// Phase 7 T7.11 — terminal manual clear. Idempotent under replay: a
/// second dispatch against an already-Revoked aggregate emits no events
/// so at-least-once delivery can't double-clear.
/// </summary>
public sealed class RevokeSanctionHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not RevokeSanctionCommand cmd)
            return;

        var aggregate = (SanctionAggregate)await context.LoadAggregateAsync(typeof(SanctionAggregate));

        if (aggregate.Status == SanctionStatus.Revoked)
            return;

        aggregate.Revoke(new Reason(cmd.RevocationReason), new Timestamp(cmd.RevokedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
