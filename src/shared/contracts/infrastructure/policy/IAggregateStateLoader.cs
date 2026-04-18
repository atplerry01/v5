namespace Whycespace.Shared.Contracts.Infrastructure.Policy;

/// <summary>
/// Phase 8 B6 — read-only seam that hydrates the current aggregate snapshot
/// for policy evaluation. Consulted by <c>PolicyMiddleware</c> immediately
/// before the OPA round-trip so rego rules that gate on
/// <c>input.resource.state</c> see the same state the engine would act on,
/// not a projection-lag-skewed read-model view.
///
/// <para>
/// <b>Fidelity contract.</b> Implementations MUST hydrate from the same
/// authoritative source the target engine uses (<c>IEventStore</c>
/// replay → aggregate reconstruction) — NOT from projections / read models.
/// The whole point of the seam is to close drift between what policy sees
/// and what the system would act on.
/// </para>
///
/// <para>
/// <b>Return shape.</b> The snapshot object is serialised verbatim onto
/// <c>input.resource.state</c> using the same snake-case
/// <c>JsonSerializerOptions</c> the command uses, so rego rules can read
/// fields like <c>input.resource.state.status</c> and
/// <c>input.resource.state.balance</c> uniformly. Implementations SHOULD
/// return a narrow record / DTO exposing only the fields policy needs — a
/// full aggregate tree MAY leak internals the policy shouldn't see.
/// </para>
///
/// <para>
/// <b>Missing-aggregate contract.</b> When no aggregate exists for the
/// given (command type, aggregate id) — factory-style commands like
/// <c>IssueSanctionCommand</c> / <c>CreateTreasuryCommand</c> — the loader
/// MUST return <c>null</c> so the middleware stamps
/// <c>input.resource.state = null</c> explicitly. Rego files handle the
/// null branch via <c>not input.resource.state</c> backward-compat rules.
/// NEVER return an empty-default record to paper over the absence — that
/// would silently satisfy state-aware deny rules.
/// </para>
///
/// <para>
/// <b>Unregistered commands.</b> The default composition wiring uses
/// <c>NullAggregateStateLoader</c> which returns <c>null</c> for every
/// input. Domains that need state-aware policy checks register a
/// per-command loader that supersedes the default.
/// </para>
/// </summary>
public interface IAggregateStateLoader
{
    /// <summary>
    /// Load the policy-visible aggregate snapshot for
    /// <paramref name="aggregateId"/> of the aggregate type associated with
    /// <paramref name="commandType"/>. Returns <c>null</c> when no
    /// aggregate exists yet (factory commands) or when no per-command
    /// loader is registered.
    /// </summary>
    Task<object?> LoadSnapshotAsync(
        Type commandType,
        Guid aggregateId,
        CancellationToken cancellationToken = default);
}
