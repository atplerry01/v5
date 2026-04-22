using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Enforcement.Sanction;

/// <summary>
/// Phase 7 T7.11 — pure, deterministic predicate for natural expiry.
///
/// Returns true iff the sanction is in <see cref="SanctionStatus.Active"/>,
/// has a bounded <see cref="EffectivePeriod.ExpiresAt"/>, and the supplied
/// <paramref name="now"/> has reached (or passed) that timestamp.
///
/// <para>
/// The predicate takes <paramref name="now"/> as an explicit argument so
/// the result is reproducible across replay without reading a wall
/// clock inside the specification. A scheduler or reactor introduced
/// later will read <c>IClock.UtcNow</c> once, pass it here, and decide
/// whether to dispatch <c>ExpireSanctionCommand</c> — the decision is
/// a pure function of aggregate state + supplied timestamp.
/// </para>
///
/// <para>
/// Open-ended sanctions (ExpiresAt = null) never naturally expire;
/// they can only terminate via Revoke.
/// </para>
/// </summary>
public static class SanctionExpirySpecification
{
    public static bool IsExpirableAt(SanctionAggregate sanction, Timestamp now)
    {
        if (sanction.Status != SanctionStatus.Active) return false;

        var expires = sanction.Period.ExpiresAt;
        if (expires is null) return false;

        return expires.Value.Value <= now.Value;
    }
}
