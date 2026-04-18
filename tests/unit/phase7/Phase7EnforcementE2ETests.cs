using Whycespace.Domain.EconomicSystem.Enforcement.Lock;
using Whycespace.Domain.EconomicSystem.Enforcement.Restriction;
using Whycespace.Domain.EconomicSystem.Enforcement.Sanction;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Tests.Shared;
using LockCause = Whycespace.Domain.EconomicSystem.Enforcement.Lock.EnforcementCause;
using LockCauseKind = Whycespace.Domain.EconomicSystem.Enforcement.Lock.EnforcementCauseKind;
using LockReason = Whycespace.Domain.EconomicSystem.Enforcement.Lock.Reason;
using LockSubjectId = Whycespace.Domain.EconomicSystem.Enforcement.Lock.SubjectId;
using RestrictionCause = Whycespace.Domain.EconomicSystem.Enforcement.Restriction.EnforcementCause;
using RestrictionCauseKind = Whycespace.Domain.EconomicSystem.Enforcement.Restriction.EnforcementCauseKind;
using RestrictionReason = Whycespace.Domain.EconomicSystem.Enforcement.Restriction.Reason;
using RestrictionSubjectId = Whycespace.Domain.EconomicSystem.Enforcement.Restriction.SubjectId;
using SanctionReason = Whycespace.Domain.EconomicSystem.Enforcement.Sanction.Reason;
using SanctionSubjectId = Whycespace.Domain.EconomicSystem.Enforcement.Sanction.SubjectId;

namespace Whycespace.Tests.Unit.Phase7;

/// <summary>
/// Phase 7 B7 — enforcement E2E validation across Restriction + Lock +
/// Sanction (B4/B5). Asserts the lifecycle completeness, cause-coupling,
/// compensation-safety suspend/resume pattern, and the authoritative
/// sanction→enforcement linkage all hold as shipped.
/// </summary>
public sealed class Phase7EnforcementE2ETests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly Timestamp T0 = new(new DateTimeOffset(2026, 4, 17, 0, 0, 0, TimeSpan.Zero));
    private static readonly Timestamp T1 = new(new DateTimeOffset(2026, 4, 17, 0, 10, 0, TimeSpan.Zero));
    private static readonly Timestamp T2 = new(new DateTimeOffset(2026, 4, 17, 0, 20, 0, TimeSpan.Zero));
    private static readonly Timestamp T3 = new(new DateTimeOffset(2026, 4, 17, 0, 30, 0, TimeSpan.Zero));

    // ══════════════════════════════════════════════════════════════════
    //  RESTRICTION (B4)
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    public void Restriction_FullLifecycle_AppliedSuspendedResumedRemoved()
    {
        var restrictionId = new RestrictionId(IdGen.Generate("R:Full:r"));
        var subjectId = new RestrictionSubjectId(IdGen.Generate("R:Full:s"));
        var applyCause = NewRestrictionCause(RestrictionCauseKind.Sanction, "R:Full:sanction");
        var suspendCause = NewRestrictionCause(RestrictionCauseKind.CompensationFlow, "R:Full:payout");

        var restriction = RestrictionAggregate.Apply(
            restrictionId, subjectId, RestrictionScope.Account,
            new RestrictionReason("manual-review"), applyCause, T0);
        Assert.Equal(RestrictionStatus.Applied, restriction.Status);

        restriction.Suspend(suspendCause, T1);
        Assert.Equal(RestrictionStatus.Suspended, restriction.Status);
        // Cause of the original apply is preserved across the suspension window.
        Assert.Equal(applyCause, restriction.Cause);

        restriction.Resume(T2);
        Assert.Equal(RestrictionStatus.Applied, restriction.Status);
        Assert.Equal(applyCause, restriction.Cause);

        restriction.Remove(new RestrictionReason("reconciliation-complete"), T3);
        Assert.Equal(RestrictionStatus.Removed, restriction.Status);
    }

    [Fact]
    public void Restriction_Suspend_FromRemoved_IsRejected()
    {
        var restriction = NewRestrictionApplied("R:Susp_FromRemoved");
        restriction.Remove(new RestrictionReason("done"), T1);

        Assert.ThrowsAny<Exception>(() =>
            restriction.Suspend(
                NewRestrictionCause(RestrictionCauseKind.Manual, "nope"), T2));
    }

    [Fact]
    public void Restriction_Resume_FromApplied_IsRejected()
    {
        var restriction = NewRestrictionApplied("R:Resume_FromApplied");
        Assert.ThrowsAny<Exception>(() => restriction.Resume(T1));
    }

    [Fact]
    public void Restriction_Update_OnSuspended_IsRejected()
    {
        var restriction = NewRestrictionApplied("R:Update_Susp");
        restriction.Suspend(
            NewRestrictionCause(RestrictionCauseKind.CompensationFlow, "c"), T1);

        Assert.ThrowsAny<Exception>(() =>
            restriction.Update(RestrictionScope.System, new RestrictionReason("x"), T2));
    }

    [Fact]
    public void Restriction_Apply_MissingCause_IsRejected()
    {
        var restrictionId = new RestrictionId(IdGen.Generate("R:NoCause:r"));
        var subjectId = new RestrictionSubjectId(IdGen.Generate("R:NoCause:s"));

        Assert.ThrowsAny<Exception>(() =>
            RestrictionAggregate.Apply(
                restrictionId, subjectId, RestrictionScope.Account,
                new RestrictionReason("r"), cause: null!, T0));
    }

    // ══════════════════════════════════════════════════════════════════
    //  LOCK (B4)
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    public void Lock_LifecycleReachingUnlocked_Released()
    {
        var lockId = new LockId(IdGen.Generate("L:Unlock:l"));
        var subjectId = new LockSubjectId(IdGen.Generate("L:Unlock:s"));
        var cause = NewLockCause(LockCauseKind.Sanction, "L:Unlock:sanction");

        var lockAgg = LockAggregate.Lock(
            lockId, subjectId, LockScope.Account,
            new LockReason("critical"), cause, T0);

        lockAgg.Suspend(
            NewLockCause(LockCauseKind.CompensationFlow, "compflow"), T1);
        lockAgg.Resume(T2);
        lockAgg.Unlock(new LockReason("investigation-complete"), T3);

        Assert.Equal(LockStatus.Unlocked, lockAgg.Status);
    }

    [Fact]
    public void Lock_LifecycleReachingExpired_NaturalExpiry()
    {
        var lockId = new LockId(IdGen.Generate("L:Expire:l"));
        var subjectId = new LockSubjectId(IdGen.Generate("L:Expire:s"));
        var cause = NewLockCause(LockCauseKind.ComplianceViolation, "L:Expire:violation");

        var lockAgg = LockAggregate.Lock(
            lockId, subjectId, LockScope.System,
            new LockReason("time-bound"), cause, T0,
            expiresAt: T3);

        lockAgg.Expire(T3);

        Assert.Equal(LockStatus.Expired, lockAgg.Status);
    }

    [Fact]
    public void Lock_Expire_FromSuspended_IsRejected()
    {
        // Suspended timer is paused — expiry rejected until Resume.
        var lockAgg = NewLockLocked("L:ExpireFromSusp", expiresAt: T2);
        lockAgg.Suspend(
            NewLockCause(LockCauseKind.CompensationFlow, "c"), T1);

        var ex = Assert.ThrowsAny<Exception>(() => lockAgg.Expire(T2));
        Assert.Contains("Suspended", ex.Message);
    }

    [Fact]
    public void Lock_Unlock_FromExpired_IsRejected()
    {
        var lockAgg = NewLockLocked("L:UnlockFromExpired", expiresAt: T2);
        lockAgg.Expire(T2);

        Assert.ThrowsAny<Exception>(() =>
            lockAgg.Unlock(new LockReason("late"), T3));
    }

    [Fact]
    public void Lock_Suspend_FromExpired_IsRejected()
    {
        var lockAgg = NewLockLocked("L:SuspFromExpired", expiresAt: T2);
        lockAgg.Expire(T2);

        Assert.ThrowsAny<Exception>(() =>
            lockAgg.Suspend(NewLockCause(LockCauseKind.Manual, "x"), T3));
    }

    [Fact]
    public void Lock_ExpiresAt_NotAfterLockedAt_IsRejected()
    {
        var cause = NewLockCause(LockCauseKind.Sanction, "bad-expiry");

        Assert.ThrowsAny<Exception>(() =>
            LockAggregate.Lock(
                new LockId(IdGen.Generate("L:BadExpiry:l")),
                new LockSubjectId(IdGen.Generate("L:BadExpiry:s")),
                LockScope.System,
                new LockReason("r"),
                cause,
                T0,
                expiresAt: T0));
    }

    [Fact]
    public void Lock_Suspend_PreservesCauseAndScopeAndReason_AcrossWindow()
    {
        var cause = NewLockCause(LockCauseKind.Sanction, "L:Preserve:sanction");
        var lockAgg = LockAggregate.Lock(
            new LockId(IdGen.Generate("L:Preserve:l")),
            new LockSubjectId(IdGen.Generate("L:Preserve:s")),
            LockScope.Capital,
            new LockReason("original-reason"),
            cause, T0, expiresAt: T3);

        lockAgg.Suspend(
            NewLockCause(LockCauseKind.CompensationFlow, "unrelated"), T1);

        // Lock-time cause, scope, reason remain exactly as set at Lock.
        Assert.Equal(cause, lockAgg.Cause);
        Assert.Equal(LockScope.Capital, lockAgg.Scope);
        Assert.Equal("original-reason", lockAgg.Reason.Value);
        Assert.Equal(T3, lockAgg.ExpiresAt);

        lockAgg.Resume(T2);

        // Resume doesn't invent new state.
        Assert.Equal(cause, lockAgg.Cause);
        Assert.Equal(LockScope.Capital, lockAgg.Scope);
        Assert.Equal("original-reason", lockAgg.Reason.Value);
        Assert.Equal(T3, lockAgg.ExpiresAt);
    }

    // ══════════════════════════════════════════════════════════════════
    //  SANCTION (B5)
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    public void Sanction_FullLifecycle_IssuedActivatedExpired_CarriesEnforcementRefAndClearedAt()
    {
        var sanctionId = new SanctionId(IdGen.Generate("S:LifeExpire:s"));
        var subjectId = new SanctionSubjectId(IdGen.Generate("S:LifeExpire:subj"));
        var period = EffectivePeriod.Bounded(T0, T3);

        var sanction = SanctionAggregate.Issue(
            sanctionId, subjectId, SanctionType.Restriction, SanctionScope.Account,
            new SanctionReason("compliance"), period, T0);

        var enforcementId = IdGen.Generate("S:LifeExpire:ref");
        var enforcementRef = new EnforcementRef(SanctionType.Restriction, enforcementId);
        sanction.Activate(enforcementRef, T1);

        Assert.Equal(SanctionStatus.Active, sanction.Status);
        Assert.Equal(enforcementRef, sanction.Enforcement);
        Assert.Null(sanction.ClearedAt);

        sanction.Expire(T3);

        Assert.Equal(SanctionStatus.Expired, sanction.Status);
        Assert.Equal(T3, sanction.ClearedAt);
    }

    [Fact]
    public void Sanction_RevokePath_PopulatesClearedAt()
    {
        var sanction = NewSanctionIssued("S:RevokePath");
        sanction.Activate(
            new EnforcementRef(SanctionType.Restriction, IdGen.Generate("s:ref")), T1);

        sanction.Revoke(new SanctionReason("manual-clear"), T2);

        Assert.Equal(SanctionStatus.Revoked, sanction.Status);
        Assert.Equal(T2, sanction.ClearedAt);
    }

    [Fact]
    public void Sanction_Activate_KindMustMatchType()
    {
        // Issue a Restriction-kind sanction; try to activate with a Lock ref.
        var sanction = NewSanctionIssuedOfType("S:KindMismatch", SanctionType.Restriction);
        var mismatchRef = new EnforcementRef(
            SanctionType.Lock,
            IdGen.Generate("S:KindMismatch:ref"));

        var ex = Assert.ThrowsAny<Exception>(() => sanction.Activate(mismatchRef, T1));
        Assert.Contains("Kind", ex.Message);
    }

    [Fact]
    public void Sanction_Activate_NullEnforcementRef_IsRejected()
    {
        var sanction = NewSanctionIssued("S:NullRef");
        Assert.ThrowsAny<Exception>(() => sanction.Activate(enforcement: null!, T1));
    }

    [Fact]
    public void Sanction_Revoke_FromExpired_IsRejected()
    {
        var sanction = NewSanctionIssued("S:RevokeFromExpired");
        sanction.Activate(
            new EnforcementRef(SanctionType.Restriction, IdGen.Generate("r")), T1);
        sanction.Expire(T2);

        Assert.ThrowsAny<Exception>(() =>
            sanction.Revoke(new SanctionReason("late"), T3));
    }

    [Fact]
    public void Sanction_Expire_FromIssued_IsRejected()
    {
        // Must go through Active before Expire.
        var sanction = NewSanctionIssued("S:ExpireFromIssued");
        Assert.ThrowsAny<Exception>(() => sanction.Expire(T1));
    }

    // ── Time-based expiry specification ──────────────────────────────

    [Fact]
    public void SanctionExpirySpec_Active_WithBoundedPeriod_ExpirableAtOrAfterExpiresAt()
    {
        var sanction = ActivateSanction("S:SpecExpirable", boundedUntil: T2);

        Assert.False(SanctionExpirySpecification.IsExpirableAt(sanction, T0));
        Assert.False(SanctionExpirySpecification.IsExpirableAt(sanction, T1));
        Assert.True(SanctionExpirySpecification.IsExpirableAt(sanction, T2));
        Assert.True(SanctionExpirySpecification.IsExpirableAt(sanction, T3));
    }

    [Fact]
    public void SanctionExpirySpec_OpenEnded_NeverExpirableNaturally()
    {
        var sanction = ActivateSanction("S:SpecOpen", boundedUntil: null);

        // Far-future `now` still evaluates false — open-ended sanctions can
        // only terminate via Revoke.
        Assert.False(SanctionExpirySpecification.IsExpirableAt(sanction, T3));
    }

    [Fact]
    public void SanctionExpirySpec_NotActive_NeverExpirable()
    {
        var sanction = NewSanctionIssued("S:SpecIssued");
        // Still Issued, not yet Active.
        Assert.False(SanctionExpirySpecification.IsExpirableAt(sanction, T3));
    }

    // ══════════════════════════════════════════════════════════════════
    //  Helpers
    // ══════════════════════════════════════════════════════════════════

    private static RestrictionCause NewRestrictionCause(
        RestrictionCauseKind kind, string seed) =>
        new(kind, IdGen.Generate($"RC:{seed}"), $"detail:{seed}");

    private static LockCause NewLockCause(
        LockCauseKind kind, string seed) =>
        new(kind, IdGen.Generate($"LC:{seed}"), $"detail:{seed}");

    private static RestrictionAggregate NewRestrictionApplied(string seed)
    {
        var id = new RestrictionId(IdGen.Generate($"{seed}:r"));
        var subj = new RestrictionSubjectId(IdGen.Generate($"{seed}:s"));
        var cause = NewRestrictionCause(RestrictionCauseKind.Sanction, $"{seed}:cause");
        return RestrictionAggregate.Apply(
            id, subj, RestrictionScope.Account,
            new RestrictionReason("r"), cause, T0);
    }

    private static LockAggregate NewLockLocked(string seed, Timestamp? expiresAt = null)
    {
        var id = new LockId(IdGen.Generate($"{seed}:l"));
        var subj = new LockSubjectId(IdGen.Generate($"{seed}:s"));
        var cause = NewLockCause(LockCauseKind.Sanction, $"{seed}:cause");
        return LockAggregate.Lock(
            id, subj, LockScope.Account,
            new LockReason("r"), cause, T0, expiresAt);
    }

    private static SanctionAggregate NewSanctionIssued(string seed) =>
        NewSanctionIssuedOfType(seed, SanctionType.Restriction);

    private static SanctionAggregate NewSanctionIssuedOfType(string seed, SanctionType type)
    {
        var id = new SanctionId(IdGen.Generate($"{seed}:s"));
        var subj = new SanctionSubjectId(IdGen.Generate($"{seed}:subj"));
        var period = EffectivePeriod.Open(T0);
        return SanctionAggregate.Issue(
            id, subj, type, SanctionScope.Account,
            new SanctionReason("r"), period, T0);
    }

    private static SanctionAggregate ActivateSanction(string seed, Timestamp? boundedUntil)
    {
        var id = new SanctionId(IdGen.Generate($"{seed}:s"));
        var subj = new SanctionSubjectId(IdGen.Generate($"{seed}:subj"));
        var period = boundedUntil.HasValue
            ? EffectivePeriod.Bounded(T0, boundedUntil.Value)
            : EffectivePeriod.Open(T0);

        var sanction = SanctionAggregate.Issue(
            id, subj, SanctionType.Restriction, SanctionScope.Account,
            new SanctionReason("r"), period, T0);
        sanction.Activate(
            new EnforcementRef(SanctionType.Restriction, IdGen.Generate($"{seed}:ref")),
            T1);
        return sanction;
    }
}
