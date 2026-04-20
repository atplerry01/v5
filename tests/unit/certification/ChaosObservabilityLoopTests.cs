using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Whycespace.Runtime.Observability;

namespace Whycespace.Tests.Unit.Certification;

/// <summary>
/// R5.C.2 Phase 1 — cross-reference validators for the chaos-observability-
/// loop catalog. Pins that every R5.B certified failure mode is covered by
/// a loop, every cited R4.A alert exists in <c>rules/*.yml</c>, every cited
/// metric matches the canonical prefix set, and every cited span family
/// matches a <c>WhyceActivitySources.Spans</c> constant.
/// </summary>
public sealed class ChaosObservabilityLoopTests
{
    private static readonly string RepoRoot = FindRepoRoot();
    private static readonly string ManifestPath = Path.Combine(
        RepoRoot, "infrastructure", "observability", "certification", "chaos-observability-loop.yml");
    private static readonly string AlertRulesRoot = Path.Combine(
        RepoRoot, "infrastructure", "observability", "prometheus", "rules");

    [Fact]
    public void Manifest_file_exists()
    {
        Assert.True(File.Exists(ManifestPath), $"Chaos observability-loop manifest missing at {ManifestPath}.");
    }

    [Fact]
    public void Every_csharp_loop_id_appears_in_yaml()
    {
        var yaml = File.ReadAllText(ManifestPath);
        foreach (var loop in CanonicalChaosLoops.All)
            Assert.Contains($"id: {loop.Id}", yaml);
    }

    [Fact]
    public void Every_certified_R5B_failure_mode_has_a_chaos_loop()
    {
        var loopFailureModeIds = CanonicalChaosLoops.All
            .Select(l => l.FailureMode)
            .ToHashSet();

        var violations = new List<string>();
        foreach (var fm in CanonicalFailureModes.All
                     .Where(m => m.Status == CanonicalFailureModes.CertifiedStatus))
        {
            if (!loopFailureModeIds.Contains(fm.Id))
                violations.Add($"R5.B certified failure mode '{fm.Id}' has no chaos-loop entry.");
        }

        // Unproven failure modes may also have loops — but that's not required.
        // The mapping check is one-way: certified ⇒ loop exists.
        Assert.True(violations.Count == 0,
            "R-CHAOS-LOOP-COVERAGE-01:\n" + string.Join("\n", violations));
    }

    [Fact]
    public void Every_chaos_loop_references_a_real_R5B_failure_mode()
    {
        var r5bIds = CanonicalFailureModes.All.Select(m => m.Id).ToHashSet();
        var violations = new List<string>();
        foreach (var loop in CanonicalChaosLoops.All)
        {
            if (!r5bIds.Contains(loop.FailureMode))
                violations.Add($"Chaos loop '{loop.Id}' references unknown R5.B failure mode '{loop.FailureMode}'.");
        }
        Assert.True(violations.Count == 0, string.Join("\n", violations));
    }

    [Fact]
    public void Every_cited_r4a_alert_exists_in_rules_files()
    {
        var actual = new HashSet<string>();
        var alertPattern = new Regex(@"^\s*- alert:\s*(?<name>\S+)", RegexOptions.Multiline);

        foreach (var file in EnumerateAlertFiles())
        {
            var text = File.ReadAllText(file);
            foreach (Match m in alertPattern.Matches(text))
                actual.Add(m.Groups["name"].Value);
        }

        var violations = new List<string>();
        foreach (var loop in CanonicalChaosLoops.All.Where(l => !string.IsNullOrWhiteSpace(l.R4aAlert)))
        {
            if (!actual.Contains(loop.R4aAlert!))
                violations.Add($"Chaos loop '{loop.Id}' cites R4.A alert '{loop.R4aAlert}' which does not exist under prometheus/rules/.");
        }
        Assert.True(violations.Count == 0,
            "R-CHAOS-LOOP-ALERT-EXISTS-01:\n" + string.Join("\n", violations));
    }

    [Fact]
    public void Every_cited_span_family_exists_as_canonical_span_constant()
    {
        var canonical = CanonicalChaosLoops.CanonicalSpanFamilies.ToHashSet();
        var violations = new List<string>();

        foreach (var loop in CanonicalChaosLoops.All)
        {
            if (!canonical.Contains(loop.SpanFamily))
                violations.Add($"Chaos loop '{loop.Id}' cites span family '{loop.SpanFamily}' which is not in the canonical WhyceActivitySources.Spans set.");
        }
        Assert.True(violations.Count == 0,
            "R-CHAOS-LOOP-SPAN-EXISTS-01:\n" + string.Join("\n", violations));
    }

    [Fact]
    public void Canonical_span_family_list_matches_WhyceActivitySources_constants()
    {
        // Anchor: the string list in CanonicalChaosLoops must stay in sync
        // with the actual span-name constants in the runtime. If a new span
        // is added or removed in WhyceActivitySources.Spans, this test flips
        // red so the chaos-loop catalog's span vocabulary updates alongside.
        var expected = new[]
        {
            WhyceActivitySources.Spans.CommandDispatch,
            WhyceActivitySources.Spans.OperatorAction,
            WhyceActivitySources.Spans.EventFabricProcess,
            WhyceActivitySources.Spans.EventFabricProcessAudit,
            WhyceActivitySources.Spans.OutboundEffectSchedule,
            WhyceActivitySources.Spans.OutboundEffectDispatch,
            WhyceActivitySources.Spans.OutboundEffectFinalize,
            WhyceActivitySources.Spans.OutboundEffectReconcile,
        };

        Assert.Equal(expected.OrderBy(s => s, StringComparer.Ordinal).ToArray(),
                     CanonicalChaosLoops.CanonicalSpanFamilies.OrderBy(s => s, StringComparer.Ordinal).ToArray());
    }

    [Fact]
    public void Every_cited_metric_matches_a_canonical_prefix()
    {
        // Reuses the canonical-prefix set already pinned by R4.A's
        // AlertExpressionMetricReferenceTests. A loop that cites a metric
        // outside this set is either a typo or an undeclared metric family.
        var prefixes = new[]
        {
            "whyce_runtime_command_",
            "whyce_runtime_operator_action_",
            "whyce_enforcement_",
            "intake_",
            "outbox_",
            "postgres_pool_",
            "event_store_",
            "policy_evaluate_",
            "chain_anchor_",
            "outbound_effect_",
            "workflow_",
            "consumer_lag_",
            "consumer_rebalance_",
            "consumer_messages_",
            "consumer_consumed_",
            "consumer_handler_",
            "consumer_dlq_",
            "projection_lag_",
            "request_",
            "error_",
        };

        var violations = new List<string>();
        foreach (var loop in CanonicalChaosLoops.All.Where(l => !string.IsNullOrWhiteSpace(l.FeedingMetric)))
        {
            if (!prefixes.Any(p => loop.FeedingMetric!.StartsWith(p, StringComparison.Ordinal)))
                violations.Add($"Chaos loop '{loop.Id}' cites metric '{loop.FeedingMetric}' which does not match any canonical prefix.");
        }
        Assert.True(violations.Count == 0,
            "R-CHAOS-LOOP-METRIC-PREFIX-01:\n" + string.Join("\n", violations));
    }

    [Fact]
    public void Every_loop_proof_status_is_cataloged_or_live_proven()
    {
        var valid = new[] { CanonicalChaosLoops.CatalogedStatus, CanonicalChaosLoops.LiveProvenStatus };
        foreach (var loop in CanonicalChaosLoops.All)
            Assert.Contains(loop.LoopProofStatus, valid);
    }

    [Fact]
    public void Every_live_proven_loop_has_existing_proof_test_file()
    {
        var violations = new List<string>();
        foreach (var loop in CanonicalChaosLoops.All
                     .Where(l => l.LoopProofStatus == CanonicalChaosLoops.LiveProvenStatus))
        {
            if (string.IsNullOrWhiteSpace(loop.ProofTest))
            {
                violations.Add($"{loop.Id}: live_proven but ProofTest is null/empty.");
                continue;
            }
            var path = Path.Combine(RepoRoot, loop.ProofTest.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(path))
                violations.Add($"{loop.Id}: proof test file missing at {path}.");
        }
        Assert.True(violations.Count == 0,
            "R-CHAOS-LOOP-LIVE-PROOF-01:\n" + string.Join("\n", violations));
    }

    [Fact]
    public void Every_cataloged_only_loop_has_null_proof_test()
    {
        foreach (var loop in CanonicalChaosLoops.All
                     .Where(l => l.LoopProofStatus == CanonicalChaosLoops.CatalogedStatus))
        {
            Assert.Null(loop.ProofTest);
        }
    }

    [Fact]
    public void Log_scope_keys_match_trace_correlation_middleware_contract()
    {
        // Pins the log-scope contract against the middleware implementation.
        // If the middleware adds / removes a key, this test fails red.
        var middlewarePath = Path.Combine(
            RepoRoot, "src", "platform", "api", "middleware", "LogCorrelationMiddleware.cs");
        var text = File.ReadAllText(middlewarePath);

        foreach (var key in CanonicalChaosLoops.RequiredLogScopeKeys)
            Assert.Contains($"\"{key}\"", text);
        foreach (var key in CanonicalChaosLoops.OptionalLogScopeKeys)
            Assert.Contains($"\"{key}\"", text);
    }

    private static IEnumerable<string> EnumerateAlertFiles()
    {
        if (!Directory.Exists(AlertRulesRoot)) yield break;
        foreach (var f in Directory.EnumerateFiles(AlertRulesRoot, "*.yml", SearchOption.AllDirectories))
            yield return f;
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
