using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Whycespace.Tests.Unit.Observability;

/// <summary>
/// R4.A / R-OBS-ASSET-PLACEMENT-01 + R-OBS-COVERAGE-01 + R-OBS-LOW-CARDINALITY-01 —
/// pins the canonical observability package. Every required dashboard JSON
/// and alert YAML file exists, parses, carries the mandatory structural
/// fields, and never references a forbidden high-cardinality label.
///
/// <para>YAML parsing is intentionally text-based — no YamlDotNet dependency
/// is introduced for these bounded assertions. We validate the shape we
/// actually depend on (group/rules/alert/expr/labels/annotations) without
/// round-tripping a full YAML AST. Prometheus validates the full schema at
/// rule-load time; this test covers asset hygiene + coverage expectations.</para>
/// </summary>
public sealed class R4AObservabilityPackageTests
{
    private static readonly string RepoRoot = FindRepoRoot();
    private static readonly string ObsRoot = Path.Combine(RepoRoot, "infrastructure", "observability");
    private static readonly string DashboardsRoot = Path.Combine(ObsRoot, "grafana", "dashboards");
    private static readonly string AlertRulesRoot = Path.Combine(ObsRoot, "prometheus", "rules");

    private static readonly string[] RequiredDashboards =
    {
        "runtime-control-plane.json",
        "workflow-runtime.json",
        "outbound-effect.json",
        "dlq-retry.json",
        "kafka-consumer-fabric.json",
        "persistence-outbox.json",
    };

    private static readonly string[] RequiredAlertFiles =
    {
        "runtime-posture.yml",
        "workflow.yml",
        "outbound-effect.yml",
        "dlq-retry.yml",
        "persistence.yml",
    };

    // R-OBS-LOW-CARDINALITY-01 — these labels are strictly forbidden on
    // dashboards and alerts. They are the canonical high-cardinality dimensions
    // the runtime vocabulary rules out (raw correlation / actor / unique
    // request ids).
    private static readonly string[] ForbiddenHighCardinalityTokens =
    {
        "correlation_id",
        "correlation.id",
        "actor_id",
        "actor.id",
        "request_id",
        "request.id",
        "command.id",
        "command_id",
    };

    [Fact]
    public void Every_required_dashboard_file_exists_and_is_valid_json()
    {
        Assert.True(Directory.Exists(DashboardsRoot), $"Dashboards directory missing at {DashboardsRoot}.");

        foreach (var file in RequiredDashboards)
        {
            var path = Path.Combine(DashboardsRoot, file);
            Assert.True(File.Exists(path), $"Required dashboard missing: {file}");

            var text = File.ReadAllText(path);
            using var doc = JsonDocument.Parse(text);
            var root = doc.RootElement;

            Assert.Equal(JsonValueKind.Object, root.ValueKind);
            Assert.True(root.TryGetProperty("title", out var title) && !string.IsNullOrWhiteSpace(title.GetString()),
                $"{file}: dashboard missing non-empty 'title'.");
            Assert.True(root.TryGetProperty("panels", out var panels) && panels.ValueKind == JsonValueKind.Array,
                $"{file}: dashboard missing 'panels' array.");
            Assert.True(panels.GetArrayLength() > 0, $"{file}: panels array is empty.");
            Assert.True(root.TryGetProperty("description", out var desc) && !string.IsNullOrWhiteSpace(desc.GetString()),
                $"{file}: dashboard missing non-empty top-level 'description' (operator purpose statement).");
        }
    }

    [Fact]
    public void Every_required_alert_rule_file_exists_and_parses()
    {
        Assert.True(Directory.Exists(AlertRulesRoot), $"Alert rules directory missing at {AlertRulesRoot}.");

        foreach (var file in RequiredAlertFiles)
        {
            var path = Path.Combine(AlertRulesRoot, file);
            Assert.True(File.Exists(path), $"Required alert rule file missing: {file}");

            var text = File.ReadAllText(path);
            Assert.Contains("groups:", text);
            Assert.Contains("- name:", text);
            Assert.Contains("rules:", text);
            Assert.Contains("- alert:", text);
            Assert.Contains("expr:", text);
            Assert.Contains("labels:", text);
            Assert.Contains("annotations:", text);
        }
    }

    [Fact]
    public void Every_alert_has_summary_and_description_annotations()
    {
        var alertStart = new Regex(@"^(\s*)- alert:\s*(\S+)", RegexOptions.Multiline);
        var violations = new System.Collections.Generic.List<string>();

        foreach (var file in RequiredAlertFiles)
        {
            var path = Path.Combine(AlertRulesRoot, file);
            var text = File.ReadAllText(path);
            var matches = alertStart.Matches(text);

            Assert.True(matches.Count > 0, $"{file}: no alert rules found.");

            foreach (Match m in matches)
            {
                var name = m.Groups[2].Value;
                // Slice the file from this alert's declaration to the next one (or EOF) —
                // annotations MUST live in that slice.
                var nextIdx = m.NextMatch().Success ? m.NextMatch().Index : text.Length;
                var slice = text.Substring(m.Index, nextIdx - m.Index);

                if (!slice.Contains("summary:"))
                    violations.Add($"{file}:{name} — missing annotations.summary.");
                if (!slice.Contains("description:"))
                    violations.Add($"{file}:{name} — missing annotations.description.");
                if (!slice.Contains("severity:"))
                    violations.Add($"{file}:{name} — missing labels.severity.");
                if (!slice.Contains("family:"))
                    violations.Add($"{file}:{name} — missing labels.family (R4.A canonical group tag).");
            }
        }

        Assert.True(violations.Count == 0,
            "R-OBS-ALERT-ACTIONABLE-01:\n" + string.Join("\n", violations));
    }

    [Fact]
    public void No_dashboard_or_alert_references_high_cardinality_labels()
    {
        var violations = new System.Collections.Generic.List<string>();

        foreach (var path in EnumerateObservabilityAssets())
        {
            var text = File.ReadAllText(path);
            foreach (var forbidden in ForbiddenHighCardinalityTokens)
            {
                if (text.Contains(forbidden, System.StringComparison.Ordinal))
                    violations.Add($"{path}: references forbidden high-cardinality token '{forbidden}'.");
            }
        }

        Assert.True(violations.Count == 0,
            "R-OBS-LOW-CARDINALITY-01:\n" + string.Join("\n", violations));
    }

    [Fact]
    public void Prometheus_config_declares_rule_files_glob()
    {
        var promCfg = Path.Combine(ObsRoot, "prometheus", "prometheus.yml");
        Assert.True(File.Exists(promCfg), $"prometheus.yml missing at {promCfg}.");
        var text = File.ReadAllText(promCfg);
        Assert.Contains("rule_files:", text);
        Assert.Contains("rules/*.yml", text);
    }

    [Fact]
    public void Grafana_dashboards_provisioning_file_exists()
    {
        var provisioningDir = Path.Combine(ObsRoot, "grafana", "provisioning", "dashboards");
        Assert.True(Directory.Exists(provisioningDir), $"Grafana dashboards provisioning dir missing: {provisioningDir}.");
        var files = Directory.GetFiles(provisioningDir, "*.yml");
        Assert.True(files.Length > 0, "No Grafana dashboard provisioning YAML file found under provisioning/dashboards/.");

        var combined = string.Join("\n", files.Select(File.ReadAllText));
        Assert.Contains("apiVersion:", combined);
        Assert.Contains("providers:", combined);
    }

    private static System.Collections.Generic.IEnumerable<string> EnumerateObservabilityAssets()
    {
        if (Directory.Exists(DashboardsRoot))
            foreach (var f in Directory.EnumerateFiles(DashboardsRoot, "*.json", SearchOption.AllDirectories))
                yield return f;
        if (Directory.Exists(AlertRulesRoot))
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
