using Whycespace.Domain.EconomicSystem.Enforcement.Lock;
using Whycespace.Domain.EconomicSystem.Enforcement.Restriction;
using Whycespace.Domain.EconomicSystem.Enforcement.Sanction;
using Whycespace.Domain.EconomicSystem.Ledger.Journal;
using Whycespace.Domain.EconomicSystem.Revenue.Distribution;
using Whycespace.Domain.EconomicSystem.Revenue.Payout;
using Whycespace.Domain.SharedKernel.Primitive.Money;
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
/// Phase 7 B7 — replay determinism + V1 backward-compatibility.
///
/// Two orthogonal properties validated here:
///   (1) Replay determinism — constructing a fresh aggregate and
///       applying a given event stream always yields the same final
///       state, regardless of construction path. Domain-level
///       counterpart of the handler-level idempotency guarantees.
///   (2) V1 backward-compat — events shaped before T7.6 / T7.10
///       extensions (no <c>Cause</c>, no <c>ExpiresAt</c>, no
///       <c>Enforcement</c> fields) replay cleanly via the
///       <c>Legacy</c> fallback synthesis; invariants hold.
/// </summary>
public sealed class Phase7ReplayDeterminismTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly Timestamp T0 = new(new DateTimeOffset(2026, 4, 17, 0, 0, 0, TimeSpan.Zero));
    private static readonly Timestamp T1 = new(new DateTimeOffset(2026, 4, 17, 0, 10, 0, TimeSpan.Zero));
    private static readonly Timestamp T2 = new(new DateTimeOffset(2026, 4, 17, 0, 20, 0, TimeSpan.Zero));
    private static readonly Timestamp T3 = new(new DateTimeOffset(2026, 4, 17, 0, 30, 0, TimeSpan.Zero));

    private static readonly Currency Usd = new("USD");

    // ══════════════════════════════════════════════════════════════════
    //  REPLAY DETERMINISM — same events, fresh aggregates, same state
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    public void Payout_ReplayOfCompensationStream_ProducesIdenticalTerminalState()
    {
        var payoutId = IdGen.Generate("Replay:Payout:id");
        var distributionId = IdGen.Generate("Replay:Payout:distribution");
        var idemKey = $"payout|{distributionId:N}|spv-1";
        var compensatingJournalId = IdGen.Generate("Replay:Payout:compjournal").ToString();

        var history = new object[]
        {
            new PayoutRequestedEvent(payoutId.ToString(), distributionId.ToString(), idemKey, T0),
            new PayoutExecutedEvent(payoutId.ToString(), distributionId.ToString())
            {
                IdempotencyKey = idemKey,
                ExecutedAt = T1,
            },
            new PayoutCompensationRequestedEvent(
                payoutId.ToString(), distributionId.ToString(), idemKey, "reason", T2),
            new PayoutCompensatedEvent(
                payoutId.ToString(), distributionId.ToString(), idemKey,
                compensatingJournalId, T3),
        };

        var first = Rehydrate<PayoutAggregate>(payoutId, history);
        var second = Rehydrate<PayoutAggregate>(payoutId, history);

        Assert.Equal(PayoutStatus.Compensated, first.Status);
        Assert.Equal(first.Status, second.Status);
        Assert.Equal(first.PayoutId, second.PayoutId);
        Assert.Equal(first.DistributionId, second.DistributionId);
        Assert.Equal(first.IdempotencyKey.Value, second.IdempotencyKey.Value);
        Assert.Equal(first.Version, second.Version);
    }

    [Fact]
    public void Distribution_ReplayOfCompensationStream_ProducesIdenticalTerminalState()
    {
        var distributionId = IdGen.Generate("Replay:Distribution:id");
        var originalPayoutId = IdGen.Generate("Replay:Distribution:payout").ToString();
        var compensatingJournalId = IdGen.Generate("Replay:Distribution:compjournal").ToString();

        var history = new object[]
        {
            new DistributionCreatedEvent(distributionId.ToString(), "spv-1", 1_000m),
            new DistributionConfirmedEvent(distributionId.ToString(), T0),
            new DistributionPaidEvent(distributionId.ToString(), originalPayoutId, T1),
            new DistributionCompensationRequestedEvent(
                distributionId.ToString(), originalPayoutId, "reason", T2),
            new DistributionCompensatedEvent(
                distributionId.ToString(), originalPayoutId, compensatingJournalId, T3),
        };

        var first = Rehydrate<DistributionAggregate>(distributionId, history);
        var second = Rehydrate<DistributionAggregate>(distributionId, history);

        Assert.Equal(DistributionStatus.Compensated, first.Status);
        Assert.Equal(first.Status, second.Status);
        Assert.Equal(first.DistributionId, second.DistributionId);
        Assert.Equal(first.Version, second.Version);
    }

    [Fact]
    public void Journal_ReplayOfCompensatingJournalStream_ProducesIdenticalState()
    {
        var compensatingId = new JournalId(IdGen.Generate("Replay:Journal:comp"));
        var originalJournalId = IdGen.Generate("Replay:Journal:original");
        var compensation = new CompensationReference(originalJournalId, "reversal");

        var history = new object[]
        {
            new JournalCompensationCreatedEvent(compensatingId, compensation, T0),
            new JournalEntryAddedEvent(
                compensatingId, IdGen.Generate("e1"), IdGen.Generate("a1"),
                new Amount(150m), Usd, BookingDirection.Debit, null, null),
            new JournalEntryAddedEvent(
                compensatingId, IdGen.Generate("e2"), IdGen.Generate("a2"),
                new Amount(150m), Usd, BookingDirection.Credit, null, null),
            new JournalPostedEvent(compensatingId, new Amount(150m), new Amount(150m), T1),
        };

        var first = Rehydrate<JournalAggregate>(compensatingId.Value, history);
        var second = Rehydrate<JournalAggregate>(compensatingId.Value, history);

        Assert.Equal(JournalKind.Compensating, first.Kind);
        Assert.Equal(first.Kind, second.Kind);
        Assert.Equal(compensation, first.Compensation);
        Assert.Equal(first.Compensation, second.Compensation);
        Assert.Equal(JournalStatus.Posted, first.Status);
        Assert.Equal(first.Status, second.Status);
        Assert.Equal(first.Version, second.Version);
    }

    // ══════════════════════════════════════════════════════════════════
    //  V1 BACKWARD-COMPATIBILITY — legacy streams still replay
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    public void Restriction_V1Replay_AppliedWithNullCause_SynthesizesLegacyCause()
    {
        var restrictionId = new RestrictionId(IdGen.Generate("V1:R:r"));
        var subjectId = new RestrictionSubjectId(IdGen.Generate("V1:R:s"));

        // V1 shape: positional constructor only, Cause init-only defaults to null.
        var history = new object[]
        {
            new RestrictionAppliedEvent(
                restrictionId, subjectId, RestrictionScope.Account,
                new RestrictionReason("legacy"), T0),
        };

        var restriction = Rehydrate<RestrictionAggregate>(restrictionId.Value, history);

        // Invariant holds: Applied state requires non-null Cause; Legacy
        // fallback ensures no violation.
        Assert.Equal(RestrictionStatus.Applied, restriction.Status);
        Assert.NotNull(restriction.Cause);
        Assert.Equal(RestrictionCauseKind.Manual, restriction.Cause!.Kind);
        Assert.Equal(subjectId.Value, restriction.Cause.CauseReferenceId);
    }

    [Fact]
    public void Restriction_MixedV1AndV2_Replay_HonoursV2CauseWhenProvided()
    {
        var restrictionId = new RestrictionId(IdGen.Generate("V1V2:R:r"));
        var subjectId = new RestrictionSubjectId(IdGen.Generate("V1V2:R:s"));
        var v2Cause = new RestrictionCause(
            RestrictionCauseKind.Sanction,
            IdGen.Generate("V1V2:R:sanction"),
            "explicit-cause");

        var history = new object[]
        {
            // V2 event — the explicit cause is preserved.
            new RestrictionAppliedEvent(
                restrictionId, subjectId, RestrictionScope.System,
                new RestrictionReason("v2-shape"), T0) { Cause = v2Cause },
        };

        var restriction = Rehydrate<RestrictionAggregate>(restrictionId.Value, history);

        Assert.Equal(v2Cause, restriction.Cause);
    }

    [Fact]
    public void Lock_V1Replay_LockedWithNullCauseAndNullExpiresAt_SynthesizesLegacy()
    {
        var lockId = new LockId(IdGen.Generate("V1:L:l"));
        var subjectId = new LockSubjectId(IdGen.Generate("V1:L:s"));

        var history = new object[]
        {
            new SystemLockedEvent(
                lockId, subjectId, LockScope.System,
                new LockReason("legacy-lock"), T0),
        };

        var lockAgg = Rehydrate<LockAggregate>(lockId.Value, history);

        Assert.Equal(LockStatus.Locked, lockAgg.Status);
        Assert.NotNull(lockAgg.Cause);
        Assert.Equal(LockCauseKind.Manual, lockAgg.Cause!.Kind);
        Assert.Equal(subjectId.Value, lockAgg.Cause.CauseReferenceId);
        Assert.Null(lockAgg.ExpiresAt); // V1 never carried an expiry
    }

    [Fact]
    public void Lock_V2Replay_HonoursCauseAndExpiresAt()
    {
        var lockId = new LockId(IdGen.Generate("V2:L:l"));
        var subjectId = new LockSubjectId(IdGen.Generate("V2:L:s"));
        var cause = new LockCause(
            LockCauseKind.ComplianceViolation,
            IdGen.Generate("V2:L:violation"),
            "legit");

        var history = new object[]
        {
            new SystemLockedEvent(
                lockId, subjectId, LockScope.Account,
                new LockReason("v2-lock"), T0) { Cause = cause, ExpiresAt = T3 },
        };

        var lockAgg = Rehydrate<LockAggregate>(lockId.Value, history);

        Assert.Equal(cause, lockAgg.Cause);
        Assert.Equal(T3, lockAgg.ExpiresAt);
    }

    [Fact]
    public void Sanction_V1Replay_ActivatedWithNullEnforcement_SynthesizesLegacyRef()
    {
        var sanctionId = new SanctionId(IdGen.Generate("V1:S:s"));
        var subjectId = new SanctionSubjectId(IdGen.Generate("V1:S:subj"));

        var history = new object[]
        {
            new SanctionIssuedEvent(
                sanctionId, subjectId, SanctionType.Restriction, SanctionScope.Account,
                new SanctionReason("legacy"), EffectivePeriod.Open(T0), T0),
            new SanctionActivatedEvent(sanctionId, subjectId, T1),
        };

        var sanction = Rehydrate<SanctionAggregate>(sanctionId.Value, history);

        Assert.Equal(SanctionStatus.Active, sanction.Status);
        Assert.NotNull(sanction.Enforcement);
        Assert.Equal(SanctionType.Restriction, sanction.Enforcement!.Kind);
        // Legacy fallback uses the SanctionId as a degenerate EnforcementId
        // marker — explicit proof that the coupling was never recorded.
        Assert.Equal(sanctionId.Value, sanction.Enforcement.EnforcementId);
    }

    [Fact]
    public void Sanction_V2Replay_HonoursEnforcementRefFromStream()
    {
        var sanctionId = new SanctionId(IdGen.Generate("V2:S:s"));
        var subjectId = new SanctionSubjectId(IdGen.Generate("V2:S:subj"));
        var enforcementRef = new EnforcementRef(
            SanctionType.Lock, IdGen.Generate("V2:S:ref"));

        var history = new object[]
        {
            new SanctionIssuedEvent(
                sanctionId, subjectId, SanctionType.Lock, SanctionScope.System,
                new SanctionReason("v2"), EffectivePeriod.Bounded(T0, T3), T0),
            new SanctionActivatedEvent(sanctionId, subjectId, T1)
            {
                Enforcement = enforcementRef
            },
        };

        var sanction = Rehydrate<SanctionAggregate>(sanctionId.Value, history);

        Assert.Equal(enforcementRef, sanction.Enforcement);
    }

    [Fact]
    public void Sanction_V1Replay_ReachingTerminal_PopulatesClearedAt()
    {
        var sanctionId = new SanctionId(IdGen.Generate("V1:S:Term:s"));
        var subjectId = new SanctionSubjectId(IdGen.Generate("V1:S:Term:subj"));

        var history = new object[]
        {
            new SanctionIssuedEvent(
                sanctionId, subjectId, SanctionType.Restriction, SanctionScope.Account,
                new SanctionReason("r"), EffectivePeriod.Bounded(T0, T2), T0),
            new SanctionActivatedEvent(sanctionId, subjectId, T1),
            new SanctionExpiredEvent(sanctionId, subjectId, T2),
        };

        var sanction = Rehydrate<SanctionAggregate>(sanctionId.Value, history);

        Assert.Equal(SanctionStatus.Expired, sanction.Status);
        Assert.Equal(T2, sanction.ClearedAt);
    }

    // ══════════════════════════════════════════════════════════════════
    //  CROSS-BATCH DETERMINISM — same seed → same id across runs
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    public void IdGenerator_SameSeed_ProducesSameGuid()
    {
        // Mirrors the deterministic-id contract every Phase 7 batch relies
        // on. Restating it here makes any accidental divergence in the
        // TestIdGenerator (or any handler that re-derives an id from an
        // aggregate field) immediately visible in the E2E suite.
        var a = IdGen.Generate("cross-batch:same-seed");
        var b = IdGen.Generate("cross-batch:same-seed");
        Assert.Equal(a, b);

        var c = IdGen.Generate("cross-batch:different-seed");
        Assert.NotEqual(a, c);
    }

    // ══════════════════════════════════════════════════════════════════
    //  Helpers
    // ══════════════════════════════════════════════════════════════════

    private static T Rehydrate<T>(Guid aggregateId, IEnumerable<object> history)
        where T : AggregateRoot
    {
        var aggregate = (T)Activator.CreateInstance(typeof(T), nonPublic: true)!;
        aggregate.HydrateIdentity(aggregateId);
        aggregate.LoadFromHistory(history);
        return aggregate;
    }
}
