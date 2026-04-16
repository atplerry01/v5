using System.Net;
using Whycespace.Platform.Api.Controllers.Economic.Compliance.Audit;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Economic.Compliance.Audit;
using Whycespace.Tests.E2E.Economic.Compliance.Setup;

namespace Whycespace.Tests.E2E.Economic.Compliance.Audit;

/// <summary>
/// End-to-end validation of the compliance/audit domain. Exercises the full
/// event → projection → query loop against the real runtime: real API host,
/// real Kafka event fabric (topic whyce.economic.compliance.audit.events),
/// real Postgres projection store (schema projection_economic_compliance_audit).
///
/// These tests are the Definition-of-Done evidence for Batch 1 (S0) of the
/// economic-system remediation plan: they prove that audit events are
/// published, consumed, stored, and queryable, and that a full audit trail
/// can be retrieved for a sample transaction.
/// </summary>
[Collection(ComplianceE2ECollection.Name)]
public sealed class AuditRecordE2ETests
{
    private const string ProjSchema = "projection_economic_compliance_audit";
    private const string ProjTable  = "audit_record_read_model";

    private readonly ComplianceE2EFixture _fix;
    public AuditRecordE2ETests(ComplianceE2EFixture fix) => _fix = fix;

    [Fact]
    public async Task Create_EmitsEvent_UpdatesProjection_QueryReturnsDraftState()
    {
        const string sourceDomain = "ledger";
        const string auditType = "Financial";
        const string evidence = "Quarterly ledger reconciliation evidence (E2E).";

        var sourceAggregateId = _fix.SeedId("audit:create:source-aggregate");
        var sourceEventId     = _fix.SeedId("audit:create:source-event");
        var correlationId     = _fix.SeedId("audit:create:corr");

        // Server-side id derivation mirrors AuditController.CreateAudit:
        //   idGenerator.Generate($"economic:compliance:audit:{SourceDomain}:{SourceAggregateId}:{SourceEventId}")
        // Both sides use TestIdGenerator / DeterministicIdGenerator (SHA-256 → Guid)
        // and identical string interpolation (Guid default "D" format), so the
        // expected id we compute here equals the one minted by the controller.
        var expectedAuditId = _fix.IdGenerator.Generate(
            $"economic:compliance:audit:{sourceDomain}:{sourceAggregateId}:{sourceEventId}");

        var post = await ComplianceApiEnvelope.PostAsync(
            _fix.Http, "/api/compliance/audit/create",
            new CreateAuditRecordRequestModel(
                sourceDomain,
                sourceAggregateId,
                sourceEventId,
                auditType,
                evidence),
            correlationId);

        Assert.Equal(HttpStatusCode.OK, post.StatusCode);
        var ack = await ComplianceApiEnvelope.ReadAsync<CommandAck>(post);
        Assert.NotNull(ack);
        Assert.True(ack!.Success);
        Assert.Equal("audit_record_created", ack.Data!.Status);

        // Wait for the Kafka consumer to materialise the Draft row.
        await ComplianceProjectionVerifier.PollUntilStatusAsync(
            ProjSchema, ProjTable, expectedAuditId, "Draft", ComplianceE2EConfig.PollTimeout);

        var get = await _fix.Http.GetAsync($"/api/compliance/audit/{expectedAuditId}");
        Assert.Equal(HttpStatusCode.OK, get.StatusCode);
        var read = await ComplianceApiEnvelope.ReadAsync<AuditRecordReadModel>(get);
        Assert.NotNull(read);
        Assert.True(read!.Success);
        Assert.Equal(expectedAuditId,  read.Data!.AuditRecordId);
        Assert.Equal(sourceDomain,     read.Data.SourceDomain);
        Assert.Equal(sourceAggregateId, read.Data.SourceAggregateId);
        Assert.Equal(sourceEventId,    read.Data.SourceEventId);
        Assert.Equal(auditType,        read.Data.AuditType);
        Assert.Equal(evidence,         read.Data.EvidenceSummary);
        Assert.Equal("Draft",          read.Data.Status);
        Assert.Null(read.Data.FinalizedAt);
    }

    [Fact]
    public async Task Lifecycle_CreateThenFinalize_ProjectionReflectsFinalizedState()
    {
        const string sourceDomain = "settlement";
        const string auditType = "Settlement";
        const string evidence = "Settlement closure evidence (E2E).";

        var sourceAggregateId = _fix.SeedId("audit:lifecycle:source-aggregate");
        var sourceEventId     = _fix.SeedId("audit:lifecycle:source-event");
        var correlationId     = _fix.SeedId("audit:lifecycle:corr");

        var expectedAuditId = _fix.IdGenerator.Generate(
            $"economic:compliance:audit:{sourceDomain}:{sourceAggregateId}:{sourceEventId}");

        // 1) Create — produces AuditRecordCreatedEvent on the audit events topic.
        var create = await ComplianceApiEnvelope.PostAsync(
            _fix.Http, "/api/compliance/audit/create",
            new CreateAuditRecordRequestModel(
                sourceDomain,
                sourceAggregateId,
                sourceEventId,
                auditType,
                evidence),
            correlationId);
        Assert.Equal(HttpStatusCode.OK, create.StatusCode);

        await ComplianceProjectionVerifier.PollUntilStatusAsync(
            ProjSchema, ProjTable, expectedAuditId, "Draft", ComplianceE2EConfig.PollTimeout);

        // 2) Finalize — produces AuditRecordFinalizedEvent; aggregate rehydrates
        //    via event replay before emitting, proving the write-side round trip.
        var finalize = await ComplianceApiEnvelope.PostAsync(
            _fix.Http, "/api/compliance/audit/finalize",
            new FinalizeAuditRecordRequestModel(expectedAuditId),
            correlationId);
        Assert.Equal(HttpStatusCode.OK, finalize.StatusCode);
        var finalizeAck = await ComplianceApiEnvelope.ReadAsync<CommandAck>(finalize);
        Assert.True(finalizeAck!.Success);
        Assert.Equal("audit_record_finalized", finalizeAck.Data!.Status);

        // 3) Projection converges to Status="Finalized".
        await ComplianceProjectionVerifier.PollUntilStatusAsync(
            ProjSchema, ProjTable, expectedAuditId, "Finalized", ComplianceE2EConfig.PollTimeout);

        // 4) Read-model GET returns the full audit trail for the transaction.
        var get = await _fix.Http.GetAsync($"/api/compliance/audit/{expectedAuditId}");
        Assert.Equal(HttpStatusCode.OK, get.StatusCode);
        var read = await ComplianceApiEnvelope.ReadAsync<AuditRecordReadModel>(get);
        Assert.True(read!.Success);
        Assert.Equal(expectedAuditId,   read.Data!.AuditRecordId);
        Assert.Equal(sourceDomain,      read.Data.SourceDomain);
        Assert.Equal(sourceAggregateId, read.Data.SourceAggregateId);
        Assert.Equal(sourceEventId,     read.Data.SourceEventId);
        Assert.Equal(auditType,         read.Data.AuditType);
        Assert.Equal(evidence,          read.Data.EvidenceSummary);
        Assert.Equal("Finalized",       read.Data.Status);
        Assert.NotNull(read.Data.FinalizedAt);
        Assert.True(read.Data.FinalizedAt >= read.Data.RecordedAt,
            $"FinalizedAt ({read.Data.FinalizedAt}) must be >= RecordedAt ({read.Data.RecordedAt}).");
    }

    [Fact]
    public async Task Finalize_UnknownAuditRecord_ReturnsError_NoProjectionRow()
    {
        var ghostAuditId  = _fix.SeedId("audit:fail:ghost");
        var correlationId = _fix.SeedId("audit:fail:corr");

        var finalize = await ComplianceApiEnvelope.PostAsync(
            _fix.Http, "/api/compliance/audit/finalize",
            new FinalizeAuditRecordRequestModel(ghostAuditId),
            correlationId);

        Assert.Equal(HttpStatusCode.BadRequest, finalize.StatusCode);

        var get = await _fix.Http.GetAsync($"/api/compliance/audit/{ghostAuditId}");
        Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);

        await ComplianceProjectionVerifier.AssertAbsentAsync(ProjSchema, ProjTable, ghostAuditId);
    }
}
