using System.Net;
using Whycespace.Platform.Api.Controllers.Economic.Reconciliation.Process;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Tests.E2E.Economic.Reconciliation.Setup;

namespace Whycespace.Tests.E2E.Economic.Reconciliation.Process;

/// <summary>
/// End-to-end validation of the reconciliation/process context. Exercises the
/// Trigger → Matched → Resolve and Trigger → Mismatched → Resolve paths through
/// the real runtime, verifying state via direct projection query (Process
/// controller has no GET endpoint by design — reconciliation is event-driven).
/// Invariants covered:
///   • reconciliation lifecycle convergence (INV-104 — every trigger reaches
///     a terminal verdict)
///   • non-converging runs surface as a policy-evaluated discrepancy, never
///     as silent data drift
///   • Resolve is terminal — reapplication rejected
/// </summary>
[Collection(ReconciliationE2ECollection.Name)]
public sealed class ReconciliationProcessE2ETests
{
    private const string ProjSchema = "projection_economic_reconciliation_process";
    private const string ProjTable  = "process_read_model";

    private readonly ReconciliationE2EFixture _fix;
    public ReconciliationProcessE2ETests(ReconciliationE2EFixture fix) => _fix = fix;

    [Fact]
    public async Task MatchedPath_TriggerMarkMatchedResolve_ProjectionConvergesToResolved()
    {
        var ledgerRef   = _fix.SeedId("match:ledger-ref");
        var observedRef = _fix.SeedId("match:observed-ref");
        var correlationId = _fix.SeedId("match:corr");

        var expectedProcessId = _fix.IdGenerator.Generate(
            $"economic:reconciliation:process:{ledgerRef}:{observedRef}");

        // 1) Trigger — Initiated.
        var trigger = await ReconciliationApiEnvelope.PostAsync(
            _fix.Http, "/api/economic/reconciliation/process/trigger",
            new TriggerReconciliationRequestModel(ledgerRef, observedRef),
            correlationId);
        Assert.Equal(HttpStatusCode.OK, trigger.StatusCode);
        var ack = await ReconciliationApiEnvelope.ReadAsync<CommandAck>(trigger);
        Assert.True(ack!.Success);
        Assert.Equal("reconciliation_triggered", ack.Data!.Status);

        await ReconciliationProjectionVerifier.PollUntilPresentAsync(
            ProjSchema, ProjTable, expectedProcessId, ReconciliationE2EConfig.PollTimeout);

        // 2) Mark matched — convergence verdict reached.
        var matched = await ReconciliationApiEnvelope.PostAsync(
            _fix.Http, "/api/economic/reconciliation/process/matched",
            new ProcessIdRequestModel(expectedProcessId),
            correlationId);
        Assert.Equal(HttpStatusCode.OK, matched.StatusCode);

        await ReconciliationProjectionVerifier.PollUntilStatusAsync(
            ProjSchema, ProjTable, expectedProcessId, "Matched", ReconciliationE2EConfig.PollTimeout);

        // 3) Resolve — terminal.
        var resolve = await ReconciliationApiEnvelope.PostAsync(
            _fix.Http, "/api/economic/reconciliation/process/resolve",
            new ProcessIdRequestModel(expectedProcessId),
            correlationId);
        Assert.Equal(HttpStatusCode.OK, resolve.StatusCode);

        await ReconciliationProjectionVerifier.PollUntilStatusAsync(
            ProjSchema, ProjTable, expectedProcessId, "Resolved", ReconciliationE2EConfig.PollTimeout);
    }

    [Fact]
    public async Task MismatchedPath_TriggerMarkMismatchedResolve_ProjectionConvergesToResolved()
    {
        var ledgerRef   = _fix.SeedId("mismatch:ledger-ref");
        var observedRef = _fix.SeedId("mismatch:observed-ref");
        var correlationId = _fix.SeedId("mismatch:corr");

        var expectedProcessId = _fix.IdGenerator.Generate(
            $"economic:reconciliation:process:{ledgerRef}:{observedRef}");

        await ReconciliationApiEnvelope.PostAsync(
            _fix.Http, "/api/economic/reconciliation/process/trigger",
            new TriggerReconciliationRequestModel(ledgerRef, observedRef),
            correlationId);
        await ReconciliationProjectionVerifier.PollUntilPresentAsync(
            ProjSchema, ProjTable, expectedProcessId, ReconciliationE2EConfig.PollTimeout);

        // Mark mismatched — discrepancy path; INV-104 demands it's surfaced,
        // not swept.
        var mismatched = await ReconciliationApiEnvelope.PostAsync(
            _fix.Http, "/api/economic/reconciliation/process/mismatched",
            new ProcessIdRequestModel(expectedProcessId),
            correlationId);
        Assert.Equal(HttpStatusCode.OK, mismatched.StatusCode);

        await ReconciliationProjectionVerifier.PollUntilStatusAsync(
            ProjSchema, ProjTable, expectedProcessId, "Mismatched", ReconciliationE2EConfig.PollTimeout);

        // Resolve — terminal even on mismatched path.
        var resolve = await ReconciliationApiEnvelope.PostAsync(
            _fix.Http, "/api/economic/reconciliation/process/resolve",
            new ProcessIdRequestModel(expectedProcessId),
            correlationId);
        Assert.Equal(HttpStatusCode.OK, resolve.StatusCode);

        await ReconciliationProjectionVerifier.PollUntilStatusAsync(
            ProjSchema, ProjTable, expectedProcessId, "Resolved", ReconciliationE2EConfig.PollTimeout);
    }

    [Fact]
    public async Task Invariant_ResolveThenResolveAgain_Rejected()
    {
        var ledgerRef   = _fix.SeedId("terminal:ledger-ref");
        var observedRef = _fix.SeedId("terminal:observed-ref");
        var correlationId = _fix.SeedId("terminal:corr");

        var expectedProcessId = _fix.IdGenerator.Generate(
            $"economic:reconciliation:process:{ledgerRef}:{observedRef}");

        await ReconciliationApiEnvelope.PostAsync(
            _fix.Http, "/api/economic/reconciliation/process/trigger",
            new TriggerReconciliationRequestModel(ledgerRef, observedRef), correlationId);
        await ReconciliationProjectionVerifier.PollUntilPresentAsync(
            ProjSchema, ProjTable, expectedProcessId, ReconciliationE2EConfig.PollTimeout);

        await ReconciliationApiEnvelope.PostAsync(
            _fix.Http, "/api/economic/reconciliation/process/matched",
            new ProcessIdRequestModel(expectedProcessId), correlationId);
        await ReconciliationProjectionVerifier.PollUntilStatusAsync(
            ProjSchema, ProjTable, expectedProcessId, "Matched", ReconciliationE2EConfig.PollTimeout);

        await ReconciliationApiEnvelope.PostAsync(
            _fix.Http, "/api/economic/reconciliation/process/resolve",
            new ProcessIdRequestModel(expectedProcessId), correlationId);
        await ReconciliationProjectionVerifier.PollUntilStatusAsync(
            ProjSchema, ProjTable, expectedProcessId, "Resolved", ReconciliationE2EConfig.PollTimeout);

        // Resolved is terminal — aggregate rejects subsequent resolve.
        var second = await ReconciliationApiEnvelope.PostAsync(
            _fix.Http, "/api/economic/reconciliation/process/resolve",
            new ProcessIdRequestModel(expectedProcessId), correlationId);
        Assert.Equal(HttpStatusCode.BadRequest, second.StatusCode);
    }

    [Fact]
    public async Task Failure_MarkMatchedUnknownProcess_ReturnsError_NoProjectionRow()
    {
        var ghostId = _fix.SeedId("process:fail:ghost");
        var correlationId = _fix.SeedId("process:fail:corr");

        var matched = await ReconciliationApiEnvelope.PostAsync(
            _fix.Http, "/api/economic/reconciliation/process/matched",
            new ProcessIdRequestModel(ghostId), correlationId);
        Assert.Equal(HttpStatusCode.BadRequest, matched.StatusCode);

        await ReconciliationProjectionVerifier.AssertAbsentAsync(ProjSchema, ProjTable, ghostId);
    }
}
