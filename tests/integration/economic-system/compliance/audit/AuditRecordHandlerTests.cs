using Whycespace.Domain.EconomicSystem.Compliance.Audit;
using Whycespace.Domain.SharedKernel.Primitive.Identity;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Engines.T2E.Economic.Compliance.Audit;
using Whycespace.Shared.Contracts.Economic.Compliance.Audit;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Integration.EconomicSystem.Compliance.Audit;

public sealed class AuditRecordHandlerTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly DateTimeOffset RecordedAt = new(2026, 4, 16, 12, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset FinalizedAt = new(2026, 4, 16, 12, 5, 0, TimeSpan.Zero);

    [Fact]
    public async Task CreateAuditRecordHandler_EmitsCreatedEvent_WithDeterministicIds()
    {
        var auditRecordId = IdGen.Generate("AuditRecordHandlerTests:Create:audit");
        var command = new CreateAuditRecordCommand(
            auditRecordId,
            "ledger",
            IdGen.Generate("AuditRecordHandlerTests:Create:source-aggregate"),
            IdGen.Generate("AuditRecordHandlerTests:Create:source-event"),
            "Financial",
            IdGen.Generate("AuditRecordHandlerTests:Create:evidence-doc").ToString(),
            RecordedAt);
        var ctx = new FakeEngineContext(command, auditRecordId);

        await new CreateAuditRecordHandler().ExecuteAsync(ctx);

        var emitted = Assert.Single(ctx.EmittedEvents);
        var evt = Assert.IsType<AuditRecordCreatedEvent>(emitted);
        Assert.Equal(auditRecordId, evt.AuditRecordId.Value);
        Assert.Equal("ledger", evt.SourceDomain.Value);
        Assert.Equal(AuditType.Financial, evt.AuditType);
    }

    [Fact]
    public async Task FinalizeAuditRecordHandler_LoadsAggregate_AndEmitsFinalizedEvent()
    {
        var auditRecordId = IdGen.Generate("AuditRecordHandlerTests:Finalize:audit");
        var draft = AuditRecordAggregate.CreateRecord(
            new AuditRecordId(auditRecordId),
            new SourceDomain("settlement"),
            new SourceAggregateId(IdGen.Generate("AuditRecordHandlerTests:Finalize:source-aggregate")),
            new SourceEventId(IdGen.Generate("AuditRecordHandlerTests:Finalize:source-event")),
            AuditType.Settlement,
            new DocumentRef(new ContentId(IdGen.Generate("AuditRecordHandlerTests:Finalize:evidence-doc"))),
            new Timestamp(RecordedAt));
        draft.ClearDomainEvents();

        var command = new FinalizeAuditRecordCommand(auditRecordId, FinalizedAt);
        var ctx = new FakeEngineContext(command, auditRecordId, preloadedAggregate: draft);

        await new FinalizeAuditRecordHandler().ExecuteAsync(ctx);

        var evt = Assert.IsType<AuditRecordFinalizedEvent>(Assert.Single(ctx.EmittedEvents));
        Assert.Equal(auditRecordId, evt.AuditRecordId.Value);
        Assert.Equal(new Timestamp(FinalizedAt), evt.FinalizedAt);
    }

    private sealed class FakeEngineContext : IEngineContext
    {
        private readonly object? _preloaded;
        private readonly List<object> _emitted = new();

        public FakeEngineContext(object command, Guid aggregateId, object? preloadedAggregate = null)
        {
            Command = command;
            AggregateId = aggregateId;
            _preloaded = preloadedAggregate;
        }

        public object Command { get; }
        public Guid AggregateId { get; }
        public string? EnforcementConstraint => null;
        public bool IsSystem => false;
        public IReadOnlyList<object> EmittedEvents => _emitted;
        public Task<object> LoadAggregateAsync(Type aggregateType) =>
            _preloaded is null
                ? throw new InvalidOperationException("FakeEngineContext: no preloaded aggregate.")
                : Task.FromResult(_preloaded);
        public void EmitEvents(IReadOnlyList<object> events) => _emitted.AddRange(events);
    }
}
