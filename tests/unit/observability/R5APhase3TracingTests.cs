using System.IO;
using Whycespace.Runtime.Observability;

namespace Whycespace.Tests.Unit.Observability;

/// <summary>
/// R5.A Phase 3 — validator tests for the outbound-effect lifecycle span
/// family (dispatch, finalize, operator reconcile).
/// </summary>
public sealed class R5APhase3TracingTests
{
    private static readonly string RepoRoot = FindRepoRoot();

    [Fact]
    public void Phase3_span_names_are_low_cardinality_constants()
    {
        Assert.Equal("outbound.effect.dispatch", WhyceActivitySources.Spans.OutboundEffectDispatch);
        Assert.Equal("outbound.effect.finalize", WhyceActivitySources.Spans.OutboundEffectFinalize);
        Assert.Equal("outbound.effect.reconcile", WhyceActivitySources.Spans.OutboundEffectReconcile);
    }

    [Fact]
    public void Phase3_attribute_keys_are_declared()
    {
        Assert.Equal("whyce.attempt.number", WhyceActivitySources.Attributes.AttemptNumber);
        Assert.Equal("whyce.finality.source", WhyceActivitySources.Attributes.FinalitySource);
        Assert.Equal("whyce.compensation.emitted", WhyceActivitySources.Attributes.CompensationEmitted);
        Assert.Equal("whyce.reconciler.actor_id", WhyceActivitySources.Attributes.ReconcilerActorId);
    }

    [Fact]
    public void OutboundEffectRelay_uses_dispatch_span_with_attempt_number()
    {
        var path = Path.Combine(RepoRoot, "src", "runtime", "outbound-effects", "OutboundEffectRelay.cs");
        var text = File.ReadAllText(path);

        Assert.Contains("WhyceActivitySources.OutboundEffects.StartActivity", text);
        Assert.Contains("WhyceActivitySources.Spans.OutboundEffectDispatch", text);
        Assert.Contains("WhyceActivitySources.Attributes.AttemptNumber", text);
        Assert.Contains("ActivityKind.Client", text);
    }

    [Fact]
    public void OutboundEffectFinalityService_uses_finalize_and_reconcile_spans()
    {
        var path = Path.Combine(RepoRoot, "src", "runtime", "outbound-effects", "OutboundEffectFinalityService.cs");
        var text = File.ReadAllText(path);

        Assert.Contains("WhyceActivitySources.Spans.OutboundEffectFinalize", text);
        Assert.Contains("WhyceActivitySources.Spans.OutboundEffectReconcile", text);
        Assert.Contains("WhyceActivitySources.Attributes.FinalitySource", text);
        Assert.Contains("WhyceActivitySources.Attributes.CompensationEmitted", text);
        Assert.Contains("WhyceActivitySources.Attributes.ReconcilerActorId", text);
    }

    [Fact]
    public void Phase3_sweeper_MarkReconciliationRequired_is_intentionally_unwrapped()
    {
        // R-TRACE-OUTBOUND-RECONCILE-SPAN-01 — the sweeper path is
        // background-worker noise and is NOT span-wrapped. This test pins
        // that choice so a well-meaning contributor doesn't add a span to
        // MarkReconciliationRequiredAsync and pollute span storage.
        var path = Path.Combine(RepoRoot, "src", "runtime", "outbound-effects", "OutboundEffectFinalityService.cs");
        var text = File.ReadAllText(path);

        var markMethodIdx = text.IndexOf(
            "public async Task MarkReconciliationRequiredAsync(",
            StringComparison.Ordinal);
        Assert.True(markMethodIdx > 0, "MarkReconciliationRequiredAsync signature not found.");

        var reconcileMethodIdx = text.IndexOf(
            "public async Task ReconcileAsync(",
            StringComparison.Ordinal);
        Assert.True(reconcileMethodIdx > markMethodIdx, "ReconcileAsync must follow MarkReconciliationRequiredAsync in source order.");

        var markBody = text.Substring(markMethodIdx, reconcileMethodIdx - markMethodIdx);
        Assert.DoesNotContain("OutboundEffects.StartActivity", markBody);
        Assert.DoesNotContain("Spans.OutboundEffectReconcile", markBody);
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
