using System.IO;
using Whycespace.Runtime.Observability;

namespace Whycespace.Tests.Unit.Observability;

/// <summary>
/// R5.A Phase 2 — validator tests for the extended tracing surface:
/// event-fabric spans, outbound-effect dispatcher span, log correlation.
/// </summary>
public sealed class R5APhase2TracingTests
{
    private static readonly string RepoRoot = FindRepoRoot();

    [Fact]
    public void Phase2_activity_source_names_are_declared()
    {
        Assert.Equal("Whycespace.Runtime.EventFabric", WhyceActivitySources.EventFabricName);
        Assert.Equal("Whycespace.Runtime.OutboundEffects", WhyceActivitySources.OutboundEffectsName);
    }

    [Fact]
    public void Phase2_span_names_are_low_cardinality_constants()
    {
        Assert.Equal("event.fabric.process", WhyceActivitySources.Spans.EventFabricProcess);
        Assert.Equal("event.fabric.process_audit", WhyceActivitySources.Spans.EventFabricProcessAudit);
        Assert.Equal("outbound.effect.schedule", WhyceActivitySources.Spans.OutboundEffectSchedule);
    }

    [Fact]
    public void Phase2_attribute_keys_are_declared()
    {
        Assert.Equal("whyce.event.count", WhyceActivitySources.Attributes.EventCount);
        Assert.Equal("whyce.provider.id", WhyceActivitySources.Attributes.ProviderId);
        Assert.Equal("whyce.effect.type", WhyceActivitySources.Attributes.EffectType);
        Assert.Equal("whyce.idempotency.key", WhyceActivitySources.Attributes.IdempotencyKey);
        Assert.Equal("whyce.dedup.hit", WhyceActivitySources.Attributes.DedupHit);
    }

    [Fact]
    public void EventFabric_uses_canonical_source_and_process_spans()
    {
        var path = Path.Combine(RepoRoot, "src", "runtime", "event-fabric", "EventFabric.cs");
        var text = File.ReadAllText(path);

        Assert.Contains("WhyceActivitySources.EventFabric.StartActivity", text);
        Assert.Contains("WhyceActivitySources.Spans.EventFabricProcess", text);
        Assert.Contains("WhyceActivitySources.Spans.EventFabricProcessAudit", text);
        Assert.Contains("WhyceActivitySources.Attributes.EventCount", text);
    }

    [Fact]
    public void OutboundEffectDispatcher_uses_canonical_source_and_schedule_span()
    {
        var path = Path.Combine(RepoRoot, "src", "runtime", "outbound-effects", "OutboundEffectDispatcher.cs");
        var text = File.ReadAllText(path);

        Assert.Contains("WhyceActivitySources.OutboundEffects.StartActivity", text);
        Assert.Contains("WhyceActivitySources.Spans.OutboundEffectSchedule", text);
        Assert.Contains("WhyceActivitySources.Attributes.ProviderId", text);
        Assert.Contains("WhyceActivitySources.Attributes.EffectType", text);
        Assert.Contains("WhyceActivitySources.Attributes.DedupHit", text);
    }

    [Fact]
    public void TracingInfrastructureModule_registers_phase2_sources()
    {
        var path = Path.Combine(RepoRoot, "src", "platform", "host", "composition",
            "infrastructure", "observability", "TracingInfrastructureModule.cs");
        var text = File.ReadAllText(path);

        Assert.Contains("WhyceActivitySources.EventFabricName", text);
        Assert.Contains("WhyceActivitySources.OutboundEffectsName", text);
    }

    [Fact]
    public void LogCorrelationMiddleware_exists_and_declares_canonical_scope_keys()
    {
        var path = Path.Combine(RepoRoot, "src", "platform", "api", "middleware", "LogCorrelationMiddleware.cs");
        Assert.True(File.Exists(path), $"LogCorrelationMiddleware.cs missing at {path}.");
        var text = File.ReadAllText(path);

        Assert.Contains("BeginScope", text);
        Assert.Contains("\"trace_id\"", text);
        Assert.Contains("\"span_id\"", text);
        Assert.Contains("\"correlation_id\"", text);
        Assert.Contains("\"tenant_id\"", text);
    }

    [Fact]
    public void LogCorrelationMiddleware_registered_after_trace_correlation_middleware()
    {
        var path = Path.Combine(RepoRoot, "src", "platform", "host", "Program.cs");
        var text = File.ReadAllText(path);

        var traceIdx = text.IndexOf("TraceCorrelationMiddleware", StringComparison.Ordinal);
        var logIdx = text.IndexOf("LogCorrelationMiddleware", StringComparison.Ordinal);
        Assert.True(logIdx > 0, "LogCorrelationMiddleware not wired in Program.cs.");
        Assert.True(traceIdx > 0 && traceIdx < logIdx,
            "LogCorrelationMiddleware must run AFTER TraceCorrelationMiddleware so the Activity trace id is populated before the log scope opens.");
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "Whycespace.sln")) &&
               !Directory.Exists(Path.Combine(dir.FullName, "src")))
        {
            dir = dir.Parent;
        }
        return dir?.FullName ?? AppContext.BaseDirectory;
    }
}
