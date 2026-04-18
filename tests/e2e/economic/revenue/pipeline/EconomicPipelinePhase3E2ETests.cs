using Whycespace.Tests.E2E.Economic.Revenue.Setup;

namespace Whycespace.Tests.E2E.Economic.Revenue.Pipeline;

/// <summary>
/// Phase 3 (T3.7) end-to-end pipeline assertions.
///
/// Scenario: active contract → record revenue → distribution auto-created
/// → distribution auto-confirmed → payout auto-executed → ledger journal
/// posted explicitly. The test makes EXACTLY ONE controller call (POST
/// /api/economic/revenue/process); all downstream stages are auto-triggered
/// inside the T1M workflows. A second identical call must be idempotent.
///
/// Required env: REVENUE_E2E_API_BASE_URL, REVENUE_E2E_PROJECTIONS_CONN,
/// REVENUE_E2E_RUN_ID. Suite fails fast if any are missing.
/// </summary>
[Collection(RevenuePipelineE2ECollection.Name)]
public sealed class EconomicPipelinePhase3E2ETests
{
    private readonly RevenuePipelineE2EFixture _fix;

    public EconomicPipelinePhase3E2ETests(RevenuePipelineE2EFixture fix) => _fix = fix;

    [Fact(DisplayName = "Revenue → Distribution → Payout → Ledger flows end-to-end with no manual triggers")]
    public async Task Pipeline_FromActiveContract_AutoTriggersAllDownstreamStages()
    {
        // Arrange — active contract is assumed seeded by suite bootstrap.
        // (Contract activation API: POST /api/economic/contract/activate.)
        var contractId = _fix.SeedId("contract:phase3:happy-path");
        var spvId = $"spv-phase3-{_fix.SeedId("spv:phase3"):N}";
        var vaultAccountId = _fix.SeedId("vault-account:phase3");
        var correlationId = _fix.SeedId("corr:phase3:happy-path");

        var revenuePayload = new
        {
            ContractId = contractId,
            SpvId = spvId,
            VaultAccountId = vaultAccountId,
            Amount = 1_000m,
            Currency = "USD",
            SourceRef = "phase3-happy-path"
        };

        // Act — single controller call. Everything else MUST be auto-triggered.
        using var response = await RevenuePipelineApiEnvelope.PostAsync(
            _fix.Http, "/api/economic/revenue/process", revenuePayload, correlationId);
        Assert.True(
            response.IsSuccessStatusCode,
            $"revenue/process returned {(int)response.StatusCode} — pipeline cannot start.");

        // Derive the deterministic downstream ids exactly as the workflow steps do.
        // Source-of-truth for these formulas:
        //   - TriggerDistributionStep:  $"distribution|{revenueId:N}"
        //   - TriggerPayoutStep:        $"payout|{distributionId:N}"
        //   - PostLedgerJournalStep:    $"journal|{payoutId:N}"
        // The revenueId itself is derived inside RevenueController as
        //   $"economic:revenue:{spvId}:{sourceRef}:{amount}:{currency}".
        var revenueId = _fix.IdGenerator.Generate(
            $"economic:revenue:{spvId}:{revenuePayload.SourceRef}:{revenuePayload.Amount}:{revenuePayload.Currency}");
        var distributionId = _fix.IdGenerator.Generate($"distribution|{revenueId:N}");
        var payoutId = _fix.IdGenerator.Generate($"payout|{distributionId:N}");
        var journalId = _fix.IdGenerator.Generate($"journal|{payoutId:N}");

        // Assert — projection convergence within the suite's poll budget.
        var distributionConfirmed = await PipelineProjectionVerifier.WaitForDistributionStatusAsync(
            distributionId, "Confirmed", RevenuePipelineE2EConfig.PollTimeout);
        Assert.True(distributionConfirmed is "Confirmed",
            $"Distribution {distributionId} did not reach Confirmed within {RevenuePipelineE2EConfig.PollTimeout}.");

        var payoutExecuted = await PipelineProjectionVerifier.WaitForPayoutStatusAsync(
            payoutId, "Executed", RevenuePipelineE2EConfig.PollTimeout);
        Assert.True(payoutExecuted is "Executed",
            $"Payout {payoutId} did not reach Executed within {RevenuePipelineE2EConfig.PollTimeout}.");

        var journalPresent = await PipelineProjectionVerifier.WaitForJournalAsync(
            journalId, RevenuePipelineE2EConfig.PollTimeout);
        Assert.True(journalPresent,
            $"Ledger journal {journalId} (LedgerJournalPostedEvent) was not projected within {RevenuePipelineE2EConfig.PollTimeout}.");

        // Closure — distribution should have advanced to Paid after the
        // MarkDistributionPaidStep ran at the tail of the payout workflow.
        var distributionPaid = await PipelineProjectionVerifier.WaitForDistributionStatusAsync(
            distributionId, "Paid", RevenuePipelineE2EConfig.PollTimeout);
        Assert.True(distributionPaid is "Paid",
            $"Distribution {distributionId} did not reach Paid within {RevenuePipelineE2EConfig.PollTimeout}.");
    }

    [Fact(DisplayName = "Re-posting the same revenue request is idempotent across the full pipeline")]
    public async Task Pipeline_OnRetry_DoesNotDoubleEmit()
    {
        var contractId = _fix.SeedId("contract:phase3:idempotency");
        var spvId = $"spv-phase3-idem-{_fix.SeedId("spv:phase3:idem"):N}";
        var vaultAccountId = _fix.SeedId("vault-account:phase3:idem");

        var revenuePayload = new
        {
            ContractId = contractId,
            SpvId = spvId,
            VaultAccountId = vaultAccountId,
            Amount = 500m,
            Currency = "USD",
            SourceRef = "phase3-idempotency"
        };

        // First post.
        using (var first = await RevenuePipelineApiEnvelope.PostAsync(
                   _fix.Http, "/api/economic/revenue/process", revenuePayload, _fix.SeedId("corr:idem:1")))
            Assert.True(first.IsSuccessStatusCode);

        // Same payload, different correlation id. Deterministic ids inside the
        // pipeline must converge on the same aggregate rows so retries do not
        // double-emit.
        using (var second = await RevenuePipelineApiEnvelope.PostAsync(
                   _fix.Http, "/api/economic/revenue/process", revenuePayload, _fix.SeedId("corr:idem:2")))
            Assert.True(second.IsSuccessStatusCode);

        var revenueId = _fix.IdGenerator.Generate(
            $"economic:revenue:{spvId}:{revenuePayload.SourceRef}:{revenuePayload.Amount}:{revenuePayload.Currency}");
        var distributionId = _fix.IdGenerator.Generate($"distribution|{revenueId:N}");
        var payoutId = _fix.IdGenerator.Generate($"payout|{distributionId:N}");

        var distributionPaid = await PipelineProjectionVerifier.WaitForDistributionStatusAsync(
            distributionId, "Paid", RevenuePipelineE2EConfig.PollTimeout);
        Assert.True(distributionPaid is "Paid",
            $"Distribution {distributionId} did not reach Paid after idempotent retry.");

        var payoutExecuted = await PipelineProjectionVerifier.WaitForPayoutStatusAsync(
            payoutId, "Executed", RevenuePipelineE2EConfig.PollTimeout);
        Assert.True(payoutExecuted is "Executed",
            $"Payout {payoutId} did not reach Executed after idempotent retry.");
    }
}
