namespace Whycespace.Shared.Contracts.Enforcement;

/// <summary>
/// Phase 8 B5 — minimal projection row the sanction expiry scheduler needs
/// to compose a deterministic ExpireSanctionCommand. Carries only the
/// authoritative aggregate id and the projected ExpiresAt so the scheduler
/// can form the idempotency key and the command payload without any
/// further projection hydration.
///
/// <para>
/// <b>ExpiresAt determinism.</b> The scheduler uses <c>ExpiresAt.UtcTicks</c>
/// as the high-resolution component of the idempotency key. Re-emitting the
/// same aggregate at the same scheduled expiry deterministically produces
/// the same key — restart / replay / multi-replica scan cannot produce a
/// second ExpireSanctionCommand for the same (SanctionId, ExpiresAt) pair.
/// </para>
/// </summary>
public sealed record ExpirableSanctionCandidate(
    Guid SanctionId,
    DateTimeOffset ExpiresAt);
