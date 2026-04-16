using Whycespace.Projections.Economic.Compliance.Audit.Reducer;
using Whycespace.Shared.Contracts.Economic.Compliance.Audit;
using Whycespace.Shared.Contracts.Events.Economic.Compliance.Audit;

namespace Whycespace.Tests.Integration.EconomicSystem.Compliance.Audit;

public sealed class AuditRecordProjectionReducerTests
{
    private static readonly Guid AggregateId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1");
    private static readonly Guid SourceAggregateId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb1");
    private static readonly Guid SourceEventId = Guid.Parse("cccccccc-cccc-cccc-cccc-ccccccccccc1");
    private static readonly DateTimeOffset RecordedAt = new(2026, 4, 16, 12, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset FinalizedAt = new(2026, 4, 16, 12, 5, 0, TimeSpan.Zero);

    private static AuditRecordCreatedEventSchema NewCreated() => new(
        AggregateId,
        "ledger",
        SourceAggregateId,
        SourceEventId,
        "Financial",
        "Quarterly ledger reconciliation evidence.",
        RecordedAt);

    private static AuditRecordFinalizedEventSchema NewFinalized() => new(AggregateId, FinalizedAt);

    [Fact]
    public void Apply_CreatedEvent_OnEmptyState_PopulatesDraftReadModel()
    {
        var state = new AuditRecordReadModel { AuditRecordId = AggregateId };

        var result = AuditRecordProjectionReducer.Apply(state, NewCreated());

        Assert.Equal(AggregateId, result.AuditRecordId);
        Assert.Equal("ledger", result.SourceDomain);
        Assert.Equal(SourceAggregateId, result.SourceAggregateId);
        Assert.Equal(SourceEventId, result.SourceEventId);
        Assert.Equal("Financial", result.AuditType);
        Assert.Equal("Quarterly ledger reconciliation evidence.", result.EvidenceSummary);
        Assert.Equal("Draft", result.Status);
        Assert.Equal(RecordedAt, result.RecordedAt);
        Assert.Equal(RecordedAt, result.LastUpdatedAt);
        Assert.Null(result.FinalizedAt);
    }

    [Fact]
    public void Apply_FinalizedEvent_OnDraftState_TransitionsToFinalized()
    {
        var draft = AuditRecordProjectionReducer.Apply(
            new AuditRecordReadModel { AuditRecordId = AggregateId },
            NewCreated());

        var result = AuditRecordProjectionReducer.Apply(draft, NewFinalized());

        Assert.Equal("Finalized", result.Status);
        Assert.Equal(FinalizedAt, result.FinalizedAt);
        Assert.Equal(FinalizedAt, result.LastUpdatedAt);
        Assert.Equal(draft.SourceDomain, result.SourceDomain);
        Assert.Equal(draft.AuditType, result.AuditType);
        Assert.Equal(draft.EvidenceSummary, result.EvidenceSummary);
        Assert.Equal(draft.RecordedAt, result.RecordedAt);
    }

    [Fact]
    public void Apply_FullEventSequence_IsDeterministicAndReplayStable()
    {
        var initial = new AuditRecordReadModel { AuditRecordId = AggregateId };

        var firstReplay = AuditRecordProjectionReducer.Apply(
            AuditRecordProjectionReducer.Apply(initial, NewCreated()),
            NewFinalized());

        var secondReplay = AuditRecordProjectionReducer.Apply(
            AuditRecordProjectionReducer.Apply(initial, NewCreated()),
            NewFinalized());

        Assert.Equal(firstReplay, secondReplay);
    }
}
