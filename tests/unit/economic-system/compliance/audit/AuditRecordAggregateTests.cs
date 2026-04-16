using Whycespace.Domain.EconomicSystem.Compliance.Audit;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.EconomicSystem.Compliance.Audit;

public sealed class AuditRecordAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly Timestamp RecordedAt  = new(new DateTimeOffset(2026, 4, 16, 12, 0, 0, TimeSpan.Zero));
    private static readonly Timestamp FinalizedAt = new(new DateTimeOffset(2026, 4, 16, 12, 5, 0, TimeSpan.Zero));

    private static AuditRecordAggregate NewDraft(string seed)
    {
        var auditRecordId     = new AuditRecordId(IdGen.Generate($"AuditRecordAggregateTests:{seed}:audit"));
        var sourceAggregateId = new SourceAggregateId(IdGen.Generate($"AuditRecordAggregateTests:{seed}:source-aggregate"));
        var sourceEventId     = new SourceEventId(IdGen.Generate($"AuditRecordAggregateTests:{seed}:source-event"));
        return AuditRecordAggregate.CreateRecord(
            auditRecordId,
            new SourceDomain("ledger"),
            sourceAggregateId,
            sourceEventId,
            AuditType.Financial,
            new EvidenceSummary("Quarterly ledger reconciliation evidence."),
            RecordedAt);
    }

    [Fact]
    public void CreateRecord_RaisesCreatedEventAndStartsInDraft()
    {
        var aggregate = NewDraft("Create_Valid");

        Assert.Equal(AuditRecordStatus.Draft, aggregate.Status);
        Assert.Equal("ledger", aggregate.SourceDomain.Value);
        Assert.Equal(AuditType.Financial, aggregate.AuditType);
        Assert.Equal(RecordedAt, aggregate.RecordedAt);

        var evt = Assert.IsType<AuditRecordCreatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal("ledger", evt.SourceDomain.Value);
        Assert.Equal("Quarterly ledger reconciliation evidence.", evt.EvidenceSummary.Value);
    }

    [Fact]
    public void Finalize_FromDraft_TransitionsToFinalizedAndRaisesEvent()
    {
        var aggregate = NewDraft("Finalize_Valid");
        aggregate.ClearDomainEvents();

        aggregate.FinalizeRecord(FinalizedAt);

        Assert.Equal(AuditRecordStatus.Finalized, aggregate.Status);
        Assert.Equal(FinalizedAt, aggregate.FinalizedAt);

        var evt = Assert.IsType<AuditRecordFinalizedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(aggregate.AuditRecordId, evt.AuditRecordId);
        Assert.Equal(FinalizedAt, evt.FinalizedAt);
    }

    [Fact]
    public void Finalize_AfterFinalize_ThrowsAlreadyFinalized()
    {
        var aggregate = NewDraft("Finalize_Twice");
        aggregate.FinalizeRecord(FinalizedAt);

        var ex = Assert.ThrowsAny<DomainException>(() => aggregate.FinalizeRecord(FinalizedAt));
        Assert.Equal(AuditErrors.AlreadyFinalized, ex.Message);
    }

    [Fact]
    public void LoadFromHistory_RehydratesTerminalStateDeterministically()
    {
        var auditRecordId     = new AuditRecordId(IdGen.Generate("AuditRecordAggregateTests:History:audit"));
        var sourceAggregateId = new SourceAggregateId(IdGen.Generate("AuditRecordAggregateTests:History:source-aggregate"));
        var sourceEventId     = new SourceEventId(IdGen.Generate("AuditRecordAggregateTests:History:source-event"));

        var history = new object[]
        {
            new AuditRecordCreatedEvent(
                auditRecordId,
                new SourceDomain("settlement"),
                sourceAggregateId,
                sourceEventId,
                AuditType.Settlement,
                new EvidenceSummary("Settlement closure evidence."),
                RecordedAt),
            new AuditRecordFinalizedEvent(auditRecordId, FinalizedAt),
        };

        var aggregate = (AuditRecordAggregate)Activator.CreateInstance(typeof(AuditRecordAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(auditRecordId, aggregate.AuditRecordId);
        Assert.Equal(AuditRecordStatus.Finalized, aggregate.Status);
        Assert.Equal(AuditType.Settlement, aggregate.AuditType);
        Assert.Equal(RecordedAt, aggregate.RecordedAt);
        Assert.Equal(FinalizedAt, aggregate.FinalizedAt);
        Assert.Empty(aggregate.DomainEvents);
    }
}
