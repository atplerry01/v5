using System.Net;
using Whycespace.Platform.Api.Controllers.Economic.Compliance.Audit;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Economic.Compliance.Audit;
using Whycespace.Tests.E2E.Economic.Compliance.Setup;

namespace Whycespace.Tests.E2E.Economic.Compliance.Audit;

/// <summary>
/// Failure-path / idempotency evidence for the compliance/audit domain.
///
/// Covers two replay scenarios that the canonical happy-path E2E does not:
///   1. <b>Deterministic ID replay</b> — two identical CreateAuditRecord
///      requests (same source triple) must collapse to a single aggregate
///      with no duplicate projection row (idempotency via deterministic id
///      seed at the controller).
///   2. <b>Terminal-state replay</b> — finalizing an already-finalized record
///      must not mutate the projection state (timestamps stay intact;
///      no drift introduced by replay).
///
/// These tests exercise the same real runtime path as the happy-path suite
/// (API → Runtime → Engine → Event Store → Kafka → Projection → Read API)
/// and assert externally-observable invariants only.
/// </summary>
[Collection(ComplianceE2ECollection.Name)]
public sealed class AuditRecordIdempotencyE2ETests
{
    private const string ProjSchema = "projection_economic_compliance_audit";
    private const string ProjTable  = "audit_record_read_model";

    private readonly ComplianceE2EFixture _fix;
    public AuditRecordIdempotencyE2ETests(ComplianceE2EFixture fix) => _fix = fix;

    [Fact]
    public async Task Idempotency_CreateWithSameSourceTriple_CollapsesToSingleAggregate()
    {
        const string sourceDomain = "ledger";
        const string auditType = "Financial";
        const string evidence = "Idempotency evidence payload.";

        var sourceAggregateId = _fix.SeedId("audit:idem:source-aggregate");
        var sourceEventId     = _fix.SeedId("audit:idem:source-event");
        var correlationId     = _fix.SeedId("audit:idem:corr");

        // The controller derives the audit record id from the source triple;
        // posting twice produces the same aggregate id both times.
        var expectedAuditId = _fix.IdGenerator.Generate(
            $"economic:compliance:audit:{sourceDomain}:{sourceAggregateId}:{sourceEventId}");

        var request = new CreateAuditRecordRequestModel(
            sourceDomain, sourceAggregateId, sourceEventId, auditType, evidence);

        // Post #1
        var first = await ComplianceApiEnvelope.PostAsync(
            _fix.Http, "/api/compliance/audit/create", request, correlationId);
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);

        await ComplianceProjectionVerifier.PollUntilStatusAsync(
            ProjSchema, ProjTable, expectedAuditId, "Draft", ComplianceE2EConfig.PollTimeout);

        // Capture projection state after first write.
        var afterFirst = await _fix.Http.GetAsync($"/api/compliance/audit/{expectedAuditId}");
        var stateFirst = await ComplianceApiEnvelope.ReadAsync<AuditRecordReadModel>(afterFirst);
        Assert.True(stateFirst!.Success);
        var recordedAtAfterFirst = stateFirst.Data!.RecordedAt;

        // Post #2 with the same triple. The controller regenerates the same
        // aggregate id; the aggregate's Create factory rejects re-creation on
        // an already-Drafted record (dedup via aggregate-level guard).
        var second = await ComplianceApiEnvelope.PostAsync(
            _fix.Http, "/api/compliance/audit/create", request, correlationId);
        Assert.Equal(HttpStatusCode.BadRequest, second.StatusCode);

        // Projection state MUST be unchanged: same RecordedAt, still Draft,
        // single row observable.
        var afterSecond = await _fix.Http.GetAsync($"/api/compliance/audit/{expectedAuditId}");
        var stateSecond = await ComplianceApiEnvelope.ReadAsync<AuditRecordReadModel>(afterSecond);
        Assert.True(stateSecond!.Success);
        Assert.Equal(expectedAuditId, stateSecond.Data!.AuditRecordId);
        Assert.Equal("Draft",          stateSecond.Data.Status);
        Assert.Equal(recordedAtAfterFirst, stateSecond.Data.RecordedAt);
    }

    [Fact]
    public async Task TerminalStateReplay_FinalizeTwice_StateUnchangedAfterRejection()
    {
        const string sourceDomain = "settlement";
        const string auditType = "Settlement";
        const string evidence = "Terminal replay evidence.";

        var sourceAggregateId = _fix.SeedId("audit:terminal:source-aggregate");
        var sourceEventId     = _fix.SeedId("audit:terminal:source-event");
        var correlationId     = _fix.SeedId("audit:terminal:corr");

        var expectedAuditId = _fix.IdGenerator.Generate(
            $"economic:compliance:audit:{sourceDomain}:{sourceAggregateId}:{sourceEventId}");

        await ComplianceApiEnvelope.PostAsync(
            _fix.Http, "/api/compliance/audit/create",
            new CreateAuditRecordRequestModel(
                sourceDomain, sourceAggregateId, sourceEventId, auditType, evidence),
            correlationId);

        await ComplianceProjectionVerifier.PollUntilStatusAsync(
            ProjSchema, ProjTable, expectedAuditId, "Draft", ComplianceE2EConfig.PollTimeout);

        // First finalize — aggregate transitions Draft → Finalized.
        var first = await ComplianceApiEnvelope.PostAsync(
            _fix.Http, "/api/compliance/audit/finalize",
            new FinalizeAuditRecordRequestModel(expectedAuditId), correlationId);
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);

        await ComplianceProjectionVerifier.PollUntilStatusAsync(
            ProjSchema, ProjTable, expectedAuditId, "Finalized", ComplianceE2EConfig.PollTimeout);

        var afterFirst = await _fix.Http.GetAsync($"/api/compliance/audit/{expectedAuditId}");
        var stateFirst = await ComplianceApiEnvelope.ReadAsync<AuditRecordReadModel>(afterFirst);
        Assert.True(stateFirst!.Success);
        var finalizedAtAfterFirst = stateFirst.Data!.FinalizedAt;
        Assert.NotNull(finalizedAtAfterFirst);

        // Second finalize — terminal-state guard rejects. Replay must not
        // overwrite the captured FinalizedAt timestamp.
        var second = await ComplianceApiEnvelope.PostAsync(
            _fix.Http, "/api/compliance/audit/finalize",
            new FinalizeAuditRecordRequestModel(expectedAuditId), correlationId);
        Assert.Equal(HttpStatusCode.BadRequest, second.StatusCode);

        var afterSecond = await _fix.Http.GetAsync($"/api/compliance/audit/{expectedAuditId}");
        var stateSecond = await ComplianceApiEnvelope.ReadAsync<AuditRecordReadModel>(afterSecond);
        Assert.True(stateSecond!.Success);
        Assert.Equal("Finalized",              stateSecond.Data!.Status);
        Assert.Equal(finalizedAtAfterFirst,    stateSecond.Data.FinalizedAt);
    }
}
