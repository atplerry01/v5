using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Whycespace.Tests.Unit.Certification;

/// <summary>
/// R5.B / R-CHAOS-ALERT-PROVENANCE-01 — every R4.A alert MUST be provenanced
/// in the failure-mode catalog: either linked to at least one canonical
/// failure mode via <c>r4a_alerts:</c>, or explicitly declared on the
/// <c>operational_only_alerts:</c> allowlist with a rationale.
///
/// <para>This closes the orphan-alert hole: an alert that fires without a
/// canonical fault in the catalog is either (a) a real gap, (b) a vanity
/// alert, or (c) a documentation lapse. All three are caught here.</para>
/// </summary>
public sealed class AlertToFailureModeMappingTests
{
    private static readonly string RepoRoot = FindRepoRoot();
    private static readonly string AlertRulesRoot = Path.Combine(
        RepoRoot, "infrastructure", "observability", "prometheus", "rules");

    [Fact]
    public void Every_alert_in_r4a_rules_is_either_linked_or_operational_only()
    {
        var provenanced = CanonicalFailureModes.All
            .SelectMany(m => m.R4aAlerts)
            .Concat(CanonicalFailureModes.OperationalOnlyAlerts.Select(o => o.Alert))
            .ToHashSet();

        var alertPattern = new Regex(@"^\s*- alert:\s*(?<name>\S+)", RegexOptions.Multiline);

        var violations = new List<string>();
        foreach (var file in EnumerateAlertFiles())
        {
            var text = File.ReadAllText(file);
            foreach (Match m in alertPattern.Matches(text))
            {
                var name = m.Groups["name"].Value;
                if (!provenanced.Contains(name))
                    violations.Add($"{Path.GetFileName(file)}: alert '{name}' has no provenance entry (link it in runtime-failure-modes.yml or add to operational_only_alerts with rationale).");
            }
        }
        Assert.True(violations.Count == 0,
            "R-CHAOS-ALERT-PROVENANCE-01:\n" + string.Join("\n", violations));
    }

    [Fact]
    public void No_linked_alert_name_is_also_in_operational_only_list()
    {
        var linked = CanonicalFailureModes.All
            .SelectMany(m => m.R4aAlerts)
            .ToHashSet();
        var operationalOnly = CanonicalFailureModes.OperationalOnlyAlerts
            .Select(o => o.Alert)
            .ToHashSet();

        var overlap = linked.Intersect(operationalOnly).ToList();
        Assert.True(overlap.Count == 0,
            "Alerts must be EITHER linked to a failure mode OR operational-only, not both: " +
            string.Join(", ", overlap));
    }

    [Fact]
    public void Every_cataloged_r4a_alert_actually_exists_in_r4a_rules()
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
        foreach (var fm in CanonicalFailureModes.All)
        {
            foreach (var a in fm.R4aAlerts)
            {
                if (!actual.Contains(a))
                    violations.Add($"{fm.Id}: cataloged alert '{a}' does not exist under infrastructure/observability/prometheus/rules/.");
            }
        }
        foreach (var op in CanonicalFailureModes.OperationalOnlyAlerts)
        {
            if (!actual.Contains(op.Alert))
                violations.Add($"operational-only alert '{op.Alert}' does not exist under infrastructure/observability/prometheus/rules/.");
        }
        Assert.True(violations.Count == 0, string.Join("\n", violations));
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
