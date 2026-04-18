using System.Diagnostics;
using Whycespace.Shared.Contracts.Operational.Sandbox.Todo;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Tests.Integration.EconomicSystem.Phase3Resilience.Shared;

namespace Whycespace.Tests.Integration.EconomicSystem.Phase3Resilience;

/// <summary>
/// Phase 3 observability gate. Proves the Phase 3 harness emits the
/// observability signals demanded by the production-grade resilience
/// contract:
///
///   O1 MetricsCollector records avg / p95 / p99 latency, throughput,
///      and error counts with per-sample fidelity.
///   O2 Tracer correlates dispatch spans by OperationId (CommandId) so
///      the received → processed → persisted chain is queryable end-to-
///      end without leaving the Phase 3 scope.
///   O3 AnomalyDetector flags latency spikes and error bursts that
///      match the thresholds declared in
///      <c>infrastructure/observability/phase3/anomaly-config.json</c>.
/// </summary>
public sealed class Phase3ObservabilityValidationTests
{
    [Fact]
    public async Task O1_MetricsCollector_Produces_Phase3_Metrics_Snapshot()
    {
        var harness = ResilienceHarness.Build();

        const int dispatchCount = 50;
        for (var i = 0; i < dispatchCount; i++)
        {
            var aggregateId = harness.IdGenerator.Generate($"phase3:observability:O1:{i}");
            var sw = Stopwatch.GetTimestamp();
            var result = await harness.ControlPlane.ExecuteAsync(
                new CreateTodoCommand(aggregateId, $"O1-{i}"),
                harness.NewTodoContext(aggregateId));
            var elapsed = Stopwatch.GetTimestamp() - sw;
            if (result.IsSuccess) harness.Metrics.RecordSuccess(elapsed);
            else harness.Metrics.RecordFailure(elapsed);
        }

        var snapshot = harness.Metrics.Snapshot();
        Assert.Equal(dispatchCount, snapshot.TotalSamples);
        Assert.Equal(dispatchCount, snapshot.SuccessCount);
        Assert.Equal(0, snapshot.FailureCount);
        Assert.True(snapshot.ThroughputPerSecond > 0);
        Assert.True(snapshot.AvgMs >= 0);
        Assert.True(snapshot.P95Ms >= snapshot.AvgMs);
        Assert.True(snapshot.P99Ms >= snapshot.P95Ms);
        Assert.True(snapshot.MaxMs >= snapshot.P99Ms);
    }

    [Fact]
    public async Task O2_Tracer_Correlates_Spans_By_OperationId()
    {
        var harness = ResilienceHarness.Build();

        var aggregateId = harness.IdGenerator.Generate("phase3:observability:O2");
        var commandId = harness.IdGenerator.Generate("phase3:observability:O2:op");
        var ctx = harness.NewTodoContext(aggregateId, commandId: commandId);

        harness.Tracer.Record(commandId, "received", ctx.CorrelationId, aggregateId);
        var result = await harness.ControlPlane.ExecuteAsync(
            new CreateTodoCommand(aggregateId, "O2"),
            ctx);
        harness.Tracer.Record(commandId, "processed", ctx.CorrelationId, aggregateId);
        if (result.IsSuccess)
            harness.Tracer.Record(commandId, "persisted", ctx.CorrelationId, aggregateId);

        var spans = harness.Tracer.SpansFor(commandId);
        Assert.Equal(3, spans.Count);
        Assert.Equal("received", spans[0].Stage);
        Assert.Equal("processed", spans[1].Stage);
        Assert.Equal("persisted", spans[2].Stage);

        foreach (var span in spans)
        {
            Assert.Equal(commandId, span.OperationId);
            Assert.Equal(ctx.CorrelationId, span.CorrelationId);
            Assert.Equal(aggregateId, span.AggregateId);
        }

        for (var i = 1; i < spans.Count; i++)
            Assert.True(spans[i].UtcTicks >= spans[i - 1].UtcTicks, "O2 spans not monotonic");
    }

    [Fact]
    public void O3_AnomalyDetector_Flags_Latency_Spikes()
    {
        var detector = new AnomalyDetector(latencySpikeMultiplier: 3.0);
        var samples = new[] { 10.0, 10.0, 10.0, 10.0, 45.0, 12.0, 11.0 };

        var anomalies = detector.DetectLatencySpikes(samples);

        Assert.Single(anomalies);
        Assert.Equal(AnomalyKind.LatencySpike, anomalies[0].Kind);
        Assert.Equal(4, anomalies[0].Index);
        Assert.Equal(45.0, anomalies[0].Value);
    }

    [Fact]
    public void O3_AnomalyDetector_Flags_Error_Bursts()
    {
        var detector = new AnomalyDetector(errorBurstWindow: TimeSpan.FromSeconds(60), errorBurstCount: 3);
        var start = new DateTimeOffset(2026, 4, 18, 12, 0, 0, TimeSpan.Zero);
        var errors = new[]
        {
            start,
            start.AddSeconds(5),
            start.AddSeconds(12),
            start.AddSeconds(600)
        };

        var anomalies = detector.DetectErrorBursts(errors);

        Assert.Single(anomalies);
        Assert.Equal(AnomalyKind.ErrorBurst, anomalies[0].Kind);
        Assert.Equal(3, anomalies[0].Value);
    }

    [Fact]
    public void O3_AnomalyDetector_Does_Not_Flag_Sparse_Errors()
    {
        var detector = new AnomalyDetector(errorBurstWindow: TimeSpan.FromSeconds(60), errorBurstCount: 3);
        var start = new DateTimeOffset(2026, 4, 18, 12, 0, 0, TimeSpan.Zero);
        var sparseErrors = new[]
        {
            start,
            start.AddSeconds(120),
            start.AddSeconds(240)
        };

        Assert.Empty(detector.DetectErrorBursts(sparseErrors));
    }

    [Fact]
    public async Task O4_Tracer_Separates_Distinct_Operations()
    {
        var harness = ResilienceHarness.Build();

        var ops = new[]
        {
            harness.IdGenerator.Generate("phase3:observability:O4:alpha"),
            harness.IdGenerator.Generate("phase3:observability:O4:beta"),
            harness.IdGenerator.Generate("phase3:observability:O4:gamma")
        };

        foreach (var opId in ops)
        {
            var aggregateId = harness.IdGenerator.Generate($"agg:{opId}");
            var ctx = harness.NewTodoContext(aggregateId, commandId: opId);
            harness.Tracer.Record(opId, "received", ctx.CorrelationId, aggregateId);
            await harness.ControlPlane.ExecuteAsync(
                new CreateTodoCommand(aggregateId, $"O4-{opId}"),
                ctx);
            harness.Tracer.Record(opId, "processed", ctx.CorrelationId, aggregateId);
        }

        Assert.Equal(ops.Length, harness.Tracer.OperationCount);
        foreach (var opId in ops)
            Assert.Equal(2, harness.Tracer.SpansFor(opId).Count);
    }
}
