using Whycespace.Domain.ControlSystem.Enforcement.Sanction;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Economic.Enforcement.Sanction;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Engines.T2E.Economic.Enforcement.Sanction;

/// <summary>
/// Phase 7 T7.10 — activates a sanction and stamps the authoritative
/// <see cref="EnforcementRef"/> onto its stream. The downstream
/// RestrictionId / LockId is derived deterministically from the
/// sanction id via <see cref="IIdGenerator"/>:
///
///   SanctionType.Restriction → IIdGenerator("restriction|sanction|{N}")
///   SanctionType.Lock        → IIdGenerator("lock|sanction|{N}")
///
/// <para>
/// Replay-convergence: the derivation is stateless and deterministic,
/// so a fresh compute on a second attempt produces the identical
/// EnforcementId that the first attempt already stamped on the event
/// stream. No projection or external join is consulted.
/// </para>
///
/// <para>
/// Idempotency: if the aggregate is already Active, the handler
/// short-circuits (no events emitted) so duplicate command delivery
/// doesn't produce a double transition. Terminal states (Expired /
/// Revoked) surface the aggregate's own rejection error via
/// <see cref="SanctionErrors.InvalidStateTransition"/>.
/// </para>
/// </summary>
public sealed class ActivateSanctionHandler : IEngine
{
    private const string RestrictionEnforcementSeedPrefix = "restriction|sanction|";
    private const string LockEnforcementSeedPrefix = "lock|sanction|";

    private readonly IIdGenerator _ids;

    public ActivateSanctionHandler(IIdGenerator ids) => _ids = ids;

    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ActivateSanctionCommand cmd)
            return;

        var aggregate = (SanctionAggregate)await context.LoadAggregateAsync(typeof(SanctionAggregate));

        // Idempotent replay short-circuit — already Active with a stream-
        // stamped EnforcementRef. Re-dispatching would either throw
        // (AlreadyActive) or attempt re-emission, neither of which is
        // correct under at-least-once delivery. Empty emission is.
        if (aggregate.Status == SanctionStatus.Active)
            return;

        var enforcement = DeriveEnforcementRef(aggregate);

        aggregate.Activate(enforcement, new Timestamp(cmd.ActivatedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }

    private EnforcementRef DeriveEnforcementRef(SanctionAggregate aggregate)
    {
        var seedPrefix = aggregate.Type switch
        {
            SanctionType.Restriction => RestrictionEnforcementSeedPrefix,
            SanctionType.Lock => LockEnforcementSeedPrefix,
            _ => throw new InvalidOperationException(
                $"Unhandled SanctionType '{aggregate.Type}' — cannot derive enforcement id.")
        };

        var enforcementId = _ids.Generate($"{seedPrefix}{aggregate.SanctionId.Value:N}");
        return new EnforcementRef(aggregate.Type, enforcementId);
    }
}
