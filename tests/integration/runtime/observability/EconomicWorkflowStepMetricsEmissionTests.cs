using NSubstitute;
using Whycespace.Engines.T1M.Domains.Economic.Revenue.Distribution.State;
using Whycespace.Engines.T1M.Domains.Economic.Revenue.Distribution.Steps;
using Whycespace.Engines.T1M.Domains.Economic.Revenue.Payout.State;
using Whycespace.Engines.T1M.Domains.Economic.Revenue.Payout.Steps;
using Whycespace.Engines.T1M.Domains.Economic.Revenue.Revenue.State;
using Whycespace.Engines.T1M.Domains.Economic.Revenue.Revenue.Steps;
using Whycespace.Shared.Contracts.Economic.Revenue.Distribution;
using Whycespace.Shared.Contracts.Economic.Revenue.Payout.Workflow;
using Whycespace.Shared.Contracts.Observability;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Integration.Runtime.Observability;

/// <summary>
/// Phase 5 — Observability & Alert Integrity.
///
/// Verifies the engine-step boundary emissions added by Phase 5 for the
/// revenue pipeline (revenue.recorded, distribution.created,
/// payout.executed). This is the T5.6 stand-in: a step-level exercise of
/// the transaction+revenue flow that asserts metric emission at each
/// boundary, without requiring the full Postgres + Kafka E2E harness.
///
/// ExecuteInstructionStep, CheckLimitStep, FxLockStep, InitiateSettlementStep,
/// and PostToLedgerStep already have direct coverage via other integration
/// tests; the gap before Phase 5 was the revenue-side emissions, which
/// this suite pins in place.
/// </summary>
public sealed class EconomicWorkflowStepMetricsEmissionTests
{
    private static readonly TestIdGenerator IdGen = new();

    private static WorkflowExecutionContext Ctx() => new()
    {
        WorkflowId = IdGen.Generate("phase5:workflow"),
        CorrelationId = IdGen.Generate("phase5:correlation"),
        WorkflowName = "test",
        Payload = new object(),
    };

    private static ISystemIntentDispatcher DispatcherReturning(CommandResult result)
    {
        var d = Substitute.For<ISystemIntentDispatcher>();
        d.DispatchSystemAsync(Arg.Any<object>(), Arg.Any<DomainRoute>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(result));
        return d;
    }

    [Fact]
    public async Task RecordRevenueStep_EmitsRevenueRecordedMetric_OnSuccess()
    {
        var metrics = Substitute.For<IEconomicMetrics>();
        var dispatcher = DispatcherReturning(CommandResult.Success(Array.Empty<object>()));
        var step = new RecordRevenueStep(dispatcher, metrics);

        var ctx = Ctx();
        ctx.SetState(new RevenueWorkflowState
        {
            RevenueId = IdGen.Generate("phase5:revenue"),
            SpvId = "spv-001",
            Amount = 500m,
            Currency = "USD",
            SourceRef = "test-src",
        });

        var result = await step.ExecuteAsync(ctx);

        Assert.True(result.IsSuccess);
        metrics.Received(1).RecordRevenueRecorded("USD", 500m);
    }

    [Fact]
    public async Task RecordRevenueStep_DoesNotEmitMetric_OnDispatchFailure()
    {
        var metrics = Substitute.For<IEconomicMetrics>();
        var dispatcher = DispatcherReturning(CommandResult.Failure("nope"));
        var step = new RecordRevenueStep(dispatcher, metrics);

        var ctx = Ctx();
        ctx.SetState(new RevenueWorkflowState
        {
            RevenueId = IdGen.Generate("phase5:revenue"),
            SpvId = "spv-001",
            Amount = 500m,
            Currency = "USD",
        });

        var result = await step.ExecuteAsync(ctx);

        Assert.False(result.IsSuccess);
        metrics.DidNotReceive().RecordRevenueRecorded(Arg.Any<string>(), Arg.Any<decimal>());
    }

    [Fact]
    public async Task CreateDistributionStep_EmitsDistributionCreatedMetric_OnSuccess()
    {
        var metrics = Substitute.For<IEconomicMetrics>();
        var dispatcher = DispatcherReturning(CommandResult.Success(Array.Empty<object>()));
        var step = new CreateDistributionStep(dispatcher, metrics);

        var ctx = Ctx();
        ctx.SetState(new DistributionWorkflowState
        {
            DistributionId = IdGen.Generate("phase5:distribution"),
            SpvId = "spv-001",
            TotalAmount = 1_000m,
            Allocations = new[]
            {
                new DistributionAllocation("participant-a", 60m),
                new DistributionAllocation("participant-b", 40m),
            },
        });

        var result = await step.ExecuteAsync(ctx);

        Assert.True(result.IsSuccess);
        metrics.Received(1).RecordDistributionCreated("spv-001", 1_000m, 2);
    }

    [Fact]
    public async Task ExecutePayoutStep_EmitsPayoutExecutedMetric_WhenAllSharesDispatchAndConserve()
    {
        var metrics = Substitute.For<IEconomicMetrics>();
        var dispatcher = DispatcherReturning(CommandResult.Success(Array.Empty<object>()));
        var step = new ExecutePayoutStep(dispatcher, metrics);

        var ctx = Ctx();
        ctx.SetState(new PayoutWorkflowState
        {
            PayoutId = IdGen.Generate("phase5:payout"),
            SpvId = "spv-001",
            SpvVaultId = IdGen.Generate("phase5:spv-vault"),
            Shares = new[]
            {
                new ParticipantPayoutEntry("participant-a", IdGen.Generate("phase5:participant-a"), 600m, 60m),
                new ParticipantPayoutEntry("participant-b", IdGen.Generate("phase5:participant-b"), 400m, 40m),
            },
        });

        var result = await step.ExecuteAsync(ctx);

        Assert.True(result.IsSuccess);
        metrics.Received(1).RecordPayoutExecuted("spv-001", 1_000m, 2);
    }
}
