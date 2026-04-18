using Whycespace.Domain.EconomicSystem.Enforcement.Lock;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Economic.Enforcement.Lock;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Infrastructure.Persistence;

namespace Whycespace.Engines.T2E.Economic.Enforcement.Lock;

/// <summary>
/// Acquires a system lock. Phase 7 T7.6/T7.9 added:
/// * Idempotent replay — duplicate <see cref="LockSystemCommand"/> on an
///   aggregate that already has an event stream short-circuits to
///   no-op.
/// * Cause-coupling — every lock records the triggering aggregate via
///   <see cref="Whycespace.Domain.EconomicSystem.Enforcement.Lock.EnforcementCause"/>.
/// * Optional natural-expiry — <see cref="LockSystemCommand.ExpiresAt"/>
///   is forwarded to the aggregate so a subsequent
///   <c>ExpireSystemLockCommand</c> can terminate the lock naturally.
/// </summary>
public sealed class LockSystemHandler : IEngine
{
    private readonly IEventStore _eventStore;

    public LockSystemHandler(IEventStore eventStore) => _eventStore = eventStore;

    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not LockSystemCommand cmd)
            return;

        // Idempotent replay short-circuit.
        var prior = await _eventStore.LoadEventsAsync(context.AggregateId);
        if (prior.Count > 0)
            return;

        if (!Enum.TryParse<LockScope>(cmd.Scope, ignoreCase: true, out var scope))
            throw new InvalidOperationException($"Unknown lock scope: '{cmd.Scope}'.");

        var cause = MapCause(cmd.Cause, cmd.SubjectId);
        Timestamp? expiresAt = cmd.ExpiresAt is DateTimeOffset exp
            ? new Timestamp(exp)
            : null;

        var aggregate = LockAggregate.Lock(
            new LockId(cmd.LockId),
            new SubjectId(cmd.SubjectId),
            scope,
            new Reason(cmd.Reason),
            cause,
            new Timestamp(cmd.LockedAt),
            expiresAt);

        context.EmitEvents(aggregate.DomainEvents);
    }

    private static EnforcementCause MapCause(EnforcementCauseDto? dto, Guid subjectId)
    {
        if (dto is null)
            return new EnforcementCause(
                EnforcementCauseKind.Manual,
                subjectId,
                "Lock command issued without explicit cause (Phase 7 T7.6 fallback).");

        if (!Enum.TryParse<EnforcementCauseKind>(dto.Kind, ignoreCase: true, out var kind))
            throw new InvalidOperationException($"Unknown EnforcementCauseKind: '{dto.Kind}'.");

        return new EnforcementCause(kind, dto.CauseReferenceId, dto.Detail);
    }
}
