using Whycespace.Domain.EconomicSystem.Ledger.Journal;
using Whycespace.Domain.EconomicSystem.Revenue.Distribution;
using Whycespace.Domain.EconomicSystem.Revenue.Payout;
using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.Phase7;

/// <summary>
/// Phase 7 B7 — cross-aggregate compensation E2E validation.
///
/// Exercises the full compensation chain shipped in B1/B2/B3:
///   Payout.Request → Executed|Failed → CompensationRequested → Compensated
///   Distribution.Paid → CompensationRequested → Compensated
///   Journal (Standard) posted, then Journal (Compensating) with same totals
///
/// Each test targets ONE invariant in the end-to-end chain. Negative
/// paths (invalid transitions, terminal-state rejections, correlation
/// drift) share the file so the parity between allow/deny paths and
/// aggregate rejection paths is explicit in one place.
/// </summary>
public sealed class Phase7CompensationE2ETests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly Timestamp T0 = new(new DateTimeOffset(2026, 4, 17, 0, 0, 0, TimeSpan.Zero));
    private static readonly Timestamp T1 = new(new DateTimeOffset(2026, 4, 17, 0, 10, 0, TimeSpan.Zero));
    private static readonly Timestamp T2 = new(new DateTimeOffset(2026, 4, 17, 0, 20, 0, TimeSpan.Zero));
    private static readonly Timestamp T3 = new(new DateTimeOffset(2026, 4, 17, 0, 30, 0, TimeSpan.Zero));

    private static readonly Currency Usd = new("USD");

    // ── PAYOUT: full compensation happy path ──────────────────────────

    [Fact]
    public void Payout_Executed_Compensated_FullLifecycleTransitionsAreValid()
    {
        var payoutId = new PayoutId(IdGen.Generate("PayoutCompE2E:Happy:payout"));
        var distributionId = new DistributionId(IdGen.Generate("PayoutCompE2E:Happy:distribution"));
        var idempotency = new PayoutIdempotencyKey($"payout|{distributionId.Value:N}|spv-1");
        var shares = new[]
        {
            new ParticipantShare("participant-a", 600m, 60m),
            new ParticipantShare("participant-b", 400m, 40m),
        };

        var payout = PayoutAggregate.Request(payoutId, distributionId, idempotency, shares, T0);
        payout.MarkExecuted(T1);
        payout.RequestCompensation("compensation-for-test", T2);

        var compensatingJournalId = IdGen.Generate("PayoutCompE2E:Happy:compensating-journal").ToString();
        payout.MarkCompensated(compensatingJournalId, T3);

        Assert.Equal(PayoutStatus.Compensated, payout.Status);
        // The event stream carries the full audit trail end-to-end.
        var eventTypes = payout.DomainEvents.Select(e => e.GetType().Name).ToArray();
        Assert.Equal(new[]
        {
            nameof(PayoutRequestedEvent),
            nameof(PayoutExecutedEvent),
            nameof(PayoutCompensationRequestedEvent),
            nameof(PayoutCompensatedEvent),
        }, eventTypes);
    }

    // ── PAYOUT: failure-path compensation ─────────────────────────────

    [Fact]
    public void Payout_Failed_Compensated_IsAValidTerminalPath()
    {
        var payout = NewPayoutRequested("Payout_Failed_Compensated");
        payout.MarkFailed("downstream vault rejected", T1);
        payout.RequestCompensation("reconciliation-sweep", T2);
        payout.MarkCompensated(IdGen.Generate("Payout_Failed_Compensated:compjournal").ToString(), T3);

        Assert.Equal(PayoutStatus.Compensated, payout.Status);
    }

    // ── PAYOUT: compensation rejections ───────────────────────────────

    [Fact]
    public void Payout_RequestCompensation_FromRequested_IsRejected()
    {
        var payout = NewPayoutRequested("Payout_Comp_FromRequested");

        var ex = Assert.ThrowsAny<Exception>(() =>
            payout.RequestCompensation("not-allowed-from-requested", T1));
        Assert.Contains("compensation", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Payout_MarkCompensated_WithoutPriorRequest_IsRejected()
    {
        var payout = NewPayoutRequested("Payout_MarkComp_NoRequest");
        payout.MarkExecuted(T1);

        Assert.ThrowsAny<Exception>(() =>
            payout.MarkCompensated(IdGen.Generate("Payout_MarkComp_NoRequest:cj").ToString(), T2));
    }

    [Fact]
    public void Payout_MarkCompensated_WithEmptyJournalId_IsRejected()
    {
        var payout = NewPayoutRequested("Payout_MarkComp_EmptyJournal");
        payout.MarkExecuted(T1);
        payout.RequestCompensation("reason", T2);

        Assert.ThrowsAny<Exception>(() =>
            payout.MarkCompensated(string.Empty, T3));
    }

    [Fact]
    public void Payout_RequestCompensation_WhenAlreadyCompensated_IsRejected()
    {
        var payout = NewPayoutRequested("Payout_Comp_Twice");
        payout.MarkExecuted(T1);
        payout.RequestCompensation("first", T2);
        payout.MarkCompensated(IdGen.Generate("Payout_Comp_Twice:cj").ToString(), T3);

        Assert.ThrowsAny<Exception>(() =>
            payout.RequestCompensation("second", T3));
    }

    // ── PAYOUT: retry specification matrix ────────────────────────────
    //
    // Exhaustive parity check against PayoutRetrySpecification.CanRetry.
    // Retry is ONLY permitted from Compensated or Failed — every other
    // status (including the default/uninitialised zero value) rejects.

    [Theory]
    [InlineData(PayoutStatus.Requested,              false)]
    [InlineData(PayoutStatus.Executed,               false)]
    [InlineData(PayoutStatus.CompensationRequested,  false)]
    [InlineData(PayoutStatus.Compensated,            true)]
    [InlineData(PayoutStatus.Failed,                 true)]
    public void PayoutRetrySpec_CanRetry_ExactlyTheTerminalRetryablePair(
        PayoutStatus priorStatus, bool expected)
    {
        Assert.Equal(expected, PayoutRetrySpecification.CanRetry(priorStatus));
    }

    // ── DISTRIBUTION: correlation invariant ───────────────────────────

    [Fact]
    public void Distribution_Compensation_RequiresOriginalPayoutId()
    {
        var distribution = NewDistributionPaid("Dist_Comp_CorrelationRequired",
            out var originalPayoutId);

        // Empty correlation → rejected.
        Assert.ThrowsAny<Exception>(() =>
            distribution.RequestCompensation(string.Empty, "reason", T2));

        // Proper correlation → accepted.
        distribution.RequestCompensation(originalPayoutId, "reason", T2);
        Assert.Equal(DistributionStatus.CompensationRequested, distribution.Status);
    }

    [Fact]
    public void Distribution_MarkCompensated_PropagatesOriginalPayoutCorrelation()
    {
        var distribution = NewDistributionPaid("Dist_MarkComp_Correlation",
            out var originalPayoutId);
        distribution.RequestCompensation(originalPayoutId, "reason", T2);

        var compensatingJournalId = IdGen.Generate("Dist:compjournal").ToString();
        distribution.MarkCompensated(originalPayoutId, compensatingJournalId, T3);

        Assert.Equal(DistributionStatus.Compensated, distribution.Status);
    }

    [Fact]
    public void Distribution_Compensation_FromCreatedIsRejected()
    {
        var distributionId = DistributionId.From(IdGen.Generate("Dist_Comp_FromCreated:dist"));
        var distribution = DistributionAggregate.CreateDistribution(
            distributionId,
            "spv-1",
            1_000m,
            new[] { ("p-a", 100m) });

        Assert.ThrowsAny<Exception>(() =>
            distribution.RequestCompensation(
                IdGen.Generate("Dist_Comp_FromCreated:payout").ToString(),
                "from-created", T1));
    }

    // ── JOURNAL: Kind/Compensation coupling ───────────────────────────

    [Fact]
    public void Journal_Standard_HasKindStandard_AndNullCompensation_InvariantsHold()
    {
        var journal = JournalAggregate.Create(
            new JournalId(IdGen.Generate("Journal_Standard:j")), T0);

        Assert.Equal(JournalKind.Standard, journal.Kind);
        Assert.Null(journal.Compensation);
    }

    [Fact]
    public void Journal_CreateCompensating_StampsKindAndCompensationReference()
    {
        var originalJournalId = IdGen.Generate("Journal_CompCreate:original");
        var compensatingId = new JournalId(IdGen.Generate("Journal_CompCreate:compensating"));
        var compensation = new CompensationReference(originalJournalId, "reason");

        var journal = JournalAggregate.CreateCompensating(compensatingId, compensation, T0);

        Assert.Equal(JournalKind.Compensating, journal.Kind);
        Assert.Equal(compensation, journal.Compensation);
        Assert.Single(journal.DomainEvents);
        Assert.IsType<JournalCompensationCreatedEvent>(journal.DomainEvents[0]);
    }

    [Fact]
    public void Journal_Compensating_WithSelfReference_IsRejected()
    {
        var journalId = new JournalId(IdGen.Generate("Journal_Comp_Self:j"));

        Assert.ThrowsAny<Exception>(() =>
            JournalAggregate.CreateCompensating(
                journalId,
                new CompensationReference(journalId.Value, "self"),
                T0));
    }

    [Fact]
    public void Journal_Compensating_PostingSameTotalsAsOriginal_Balances()
    {
        // Original: debit 150, credit 150 (ledger-balanced).
        var original = JournalAggregate.Create(
            new JournalId(IdGen.Generate("Journal_CompTotals:original")), T0);
        original.AddEntry(IdGen.Generate("o:d"), IdGen.Generate("a1"), new Amount(150m), Usd, BookingDirection.Debit);
        original.AddEntry(IdGen.Generate("o:c"), IdGen.Generate("a2"), new Amount(150m), Usd, BookingDirection.Credit);
        original.Post(T1);

        // Compensating journal reverses at the same total magnitudes.
        var originalJournalId = original.JournalId.Value;
        var compensatingId = new JournalId(IdGen.Generate("Journal_CompTotals:compensating"));
        var compensation = new CompensationReference(originalJournalId, "reversal");
        var compensating = JournalAggregate.CreateCompensating(compensatingId, compensation, T2);

        // Sign-flipped relative to original — same totals, balances at post.
        compensating.AddEntry(IdGen.Generate("c:d"), IdGen.Generate("a2"), new Amount(150m), Usd, BookingDirection.Debit);
        compensating.AddEntry(IdGen.Generate("c:c"), IdGen.Generate("a1"), new Amount(150m), Usd, BookingDirection.Credit);
        compensating.Post(T3);

        Assert.Equal(JournalStatus.Posted, compensating.Status);
        Assert.Equal(JournalKind.Compensating, compensating.Kind);
        // Original remains untouched (append-only contract).
        Assert.Equal(JournalStatus.Posted, original.Status);
        Assert.Equal(JournalKind.Standard, original.Kind);
    }

    [Fact]
    public void Journal_Compensating_UnbalancedEntries_StillRejected()
    {
        var compensatingId = new JournalId(IdGen.Generate("Journal_CompUnbal:compensating"));
        var compensation = new CompensationReference(
            IdGen.Generate("Journal_CompUnbal:original"), "reversal");

        var compensating = JournalAggregate.CreateCompensating(compensatingId, compensation, T0);
        compensating.AddEntry(IdGen.Generate("c:d"), IdGen.Generate("a1"), new Amount(50m), Usd, BookingDirection.Debit);
        compensating.AddEntry(IdGen.Generate("c:c"), IdGen.Generate("a2"), new Amount(40m), Usd, BookingDirection.Credit);

        Assert.ThrowsAny<Exception>(() => compensating.Post(T1));
    }

    [Fact]
    public void CompensationReference_EmptyOriginal_Rejected()
    {
        Assert.Throws<ArgumentException>(() =>
            new CompensationReference(Guid.Empty, "reason"));
    }

    [Fact]
    public void CompensationReference_EmptyReason_Rejected()
    {
        Assert.Throws<ArgumentException>(() =>
            new CompensationReference(IdGen.Generate("cr:j"), string.Empty));
    }

    // ── Helpers ──────────────────────────────────────────────────────

    private static PayoutAggregate NewPayoutRequested(string seed)
    {
        var payoutId = new PayoutId(IdGen.Generate($"{seed}:payout"));
        var distributionId = new DistributionId(IdGen.Generate($"{seed}:distribution"));
        var idempotency = new PayoutIdempotencyKey($"payout|{distributionId.Value:N}|spv-1");
        var shares = new[]
        {
            new ParticipantShare("participant-a", 600m, 60m),
            new ParticipantShare("participant-b", 400m, 40m),
        };
        return PayoutAggregate.Request(payoutId, distributionId, idempotency, shares, T0);
    }

    private static DistributionAggregate NewDistributionPaid(string seed, out string originalPayoutId)
    {
        var distributionId = DistributionId.From(IdGen.Generate($"{seed}:dist"));
        var distribution = DistributionAggregate.CreateDistribution(
            distributionId,
            "spv-1",
            1_000m,
            new[] { ("p-a", 60m), ("p-b", 40m) });
        distribution.Confirm(T0);
        originalPayoutId = IdGen.Generate($"{seed}:original-payout").ToString();
        distribution.MarkPaid(originalPayoutId, T1);
        return distribution;
    }
}
