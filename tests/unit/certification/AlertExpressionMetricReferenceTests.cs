using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Whycespace.Tests.Unit.Certification;

/// <summary>
/// R5.B / R-CHAOS-ALERT-EXPR-SANITY-01 — every PromQL expression in an R4.A
/// alert file MUST reference only metric families we know the runtime emits.
/// Prometheus would catch missing metrics at evaluation time, but only if the
/// alert actually runs — a typo in a critical alert could silently never fire.
///
/// <para>The test extracts bareword identifiers from each PromQL expression,
/// filters out known PromQL keywords / functions / label operators, and
/// verifies every remaining identifier matches one of the canonical metric
/// prefixes. prometheus-net collapses dots/dashes to underscores and appends
/// _total (counters) / _bucket/_sum/_count (histograms), so we match by
/// prefix.</para>
/// </summary>
public sealed class AlertExpressionMetricReferenceTests
{
    private static readonly string RepoRoot = FindRepoRoot();
    private static readonly string AlertRulesRoot = Path.Combine(
        RepoRoot, "infrastructure", "observability", "prometheus", "rules");

    // Canonical metric prefixes. Derived from the R4.A recon + source meter
    // declarations. Every alert expression's metric reference MUST start with
    // one of these prefixes (after prometheus-net's dot→underscore collapse).
    private static readonly string[] CanonicalMetricPrefixes =
    {
        // Whycespace.Runtime.ControlPlane
        "whyce_runtime_command_",
        "whyce_runtime_operator_action_",
        // Whycespace.Runtime.Enforcement
        "whyce_enforcement_",
        // Whycespace.Intake
        "intake_",
        // Whycespace.Outbox
        "outbox_",
        // Whycespace.Postgres
        "postgres_pool_",
        // Whycespace.EventStore
        "event_store_",
        // Whycespace.Policy
        "policy_evaluate_",
        // Whycespace.Chain
        "chain_anchor_",
        // Whycespace.OutboundEffects
        "outbound_effect_",
        // Whycespace.Workflow
        "workflow_",
        // Whycespace.Kafka.Consumer
        "consumer_lag_",
        "consumer_rebalance_",
        "consumer_messages_",
        // Whycespace.Projection.Consumer
        "consumer_consumed_",
        "consumer_handler_",
        "consumer_dlq_",
        "projection_lag_",
        // prometheus-net legacy HTTP
        "request_",
        "error_",
        "policy_evaluation_duration_",
        "engine_execution_duration_",
        // stdlib / process / up
        "up",
        "process_",
    };

    // PromQL functions + keywords + unit suffixes. Everything in this set is
    // NOT a metric name and must be filtered out before the prefix check.
    private static readonly HashSet<string> PromqlLexicon = new(StringComparer.Ordinal)
    {
        "sum", "rate", "increase", "histogram_quantile", "clamp_min", "clamp_max",
        "avg", "min", "max", "count", "stddev", "stdvar", "topk", "bottomk",
        "by", "without", "offset", "on", "ignoring", "group_left", "group_right",
        "if", "unless", "and", "or", "vector", "scalar", "time",
        "le", "for", "labels", "annotations", "alert", "expr", "severity",
        "family", "summary", "description",
        // numeric-ish tokens we don't want to flag
        "bool",
    };

    [Fact]
    public void Every_alert_expression_metric_reference_is_in_canonical_set()
    {
        // `- alert: X` followed by `expr: <one-line>` — the alerts in R4.A are
        // single-line expressions. Multi-line exprs would require a richer
        // parser; none of our R4.A alerts use them.
        var exprPattern = new Regex(@"^\s+expr:\s*(?<body>.+)$", RegexOptions.Multiline);
        var identifierPattern = new Regex(@"(?<![A-Za-z_0-9])([A-Za-z_][A-Za-z_0-9]*)(?![A-Za-z_0-9])");

        var violations = new List<string>();
        foreach (var file in EnumerateAlertFiles())
        {
            var text = File.ReadAllText(file);
            foreach (Match m in exprPattern.Matches(text))
            {
                var body = m.Groups["body"].Value;

                // Strip string-literal label values so we don't flag e.g.
                // action_type="dlq.redrive" 's "dlq.redrive" as a metric.
                var stripped = Regex.Replace(body, "\"[^\"]*\"", "\"\"");

                // Strip the label list inside `by (...)` / `without (...)` /
                // `on (...)` / `group_left (...)` / `group_right (...)` — those
                // identifiers are label names, not metric names. Replacing
                // them with empty parens keeps surrounding structure intact.
                var noGroupings = Regex.Replace(
                    stripped,
                    @"\b(by|without|on|ignoring|group_left|group_right)\s*\([^)]*\)",
                    "$1 ()");

                foreach (Match idMatch in identifierPattern.Matches(noGroupings))
                {
                    var id = idMatch.Groups[1].Value;

                    // Skip pure numbers (the identifier regex already excludes
                    // them, but defense in depth).
                    if (id.All(char.IsDigit)) continue;

                    // Skip PromQL lexicon.
                    if (PromqlLexicon.Contains(id)) continue;

                    // Skip label keys (preceding `{` or `,` followed by `=`).
                    // Simplest signal: the identifier is followed by an
                    // equals sign in the expression context.
                    var idIdx = idMatch.Index;
                    var afterId = idIdx + id.Length;
                    if (afterId < noGroupings.Length && noGroupings[afterId] is '=' or ' ' && TokenLooksLikeLabelKey(noGroupings, idIdx, afterId))
                        continue;

                    // Skip tokens that are part of a label-selector body
                    // (values on the right of `=`, `!=`, `=~`, `!~`).
                    if (IsLabelSelectorValue(noGroupings, idIdx)) continue;

                    // Now the remaining bareword identifiers must match a
                    // canonical metric prefix.
                    if (!CanonicalMetricPrefixes.Any(p => id.StartsWith(p, StringComparison.Ordinal))
                        && id != "up")
                        violations.Add($"{Path.GetFileName(file)}: identifier '{id}' in expr '{body.Trim()}' does not match any canonical metric prefix.");
                }
            }
        }

        Assert.True(violations.Count == 0,
            "R-CHAOS-ALERT-EXPR-SANITY-01:\n" + string.Join("\n", violations.Distinct()));
    }

    private static bool TokenLooksLikeLabelKey(string text, int idStart, int afterId)
    {
        // Walk forward skipping spaces; if we hit `=` or `=~` or `!=` or `!~`
        // the token is a label key.
        for (var i = afterId; i < text.Length; i++)
        {
            var c = text[i];
            if (c == ' ') continue;
            if (c == '=' || c == '!') return true;
            return false;
        }
        return false;
    }

    private static bool IsLabelSelectorValue(string text, int idStart)
    {
        // Walk backward skipping spaces and quotes; if we're inside `{…}`
        // and immediately after `=`, `=~`, `!=`, or `!~`, we're a value.
        var depth = 0;
        for (var i = idStart - 1; i >= 0; i--)
        {
            var c = text[i];
            if (c == '}') depth++;
            if (c == '{')
            {
                depth--;
                if (depth < 0) return false; // went past the opening brace
            }
            if (c == '=' && depth == 0 && IsInsideSelector(text, i))
                return true;
        }
        return false;
    }

    private static bool IsInsideSelector(string text, int eqIdx)
    {
        // True if the `=` at eqIdx is inside a `{ … }` label-selector.
        var openIdx = text.LastIndexOf('{', eqIdx);
        var closeIdx = text.LastIndexOf('}', eqIdx);
        return openIdx > closeIdx;
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
