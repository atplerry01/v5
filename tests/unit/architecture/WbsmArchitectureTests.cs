using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Whycespace.Tests.Unit.Architecture;

/// <summary>
/// WBSM v3.5 hardening — H1 assertion gate.
///
/// These tests pin the architectural invariants that phase-1 (S0–S8) and the
/// v3.5 hardening pass have already established as compliant. They exist to
/// detect future drift, not to fix current violations.
///
/// All scans are text-based against source files on disk — they intentionally
/// avoid reflection so the unit test project does not need to reference
/// runtime/platform/projections.
/// </summary>
public sealed class WbsmArchitectureTests
{
    private static readonly string RepoRoot = FindRepoRoot();
    private static readonly string SrcRoot = Path.Combine(RepoRoot, "src");

    // ─────────────────────────────────────────────────────────────────────
    // §1 Determinism — domain layer purity
    // ─────────────────────────────────────────────────────────────────────

    [Fact]
    public void Domain_layer_contains_no_GuidNewGuid_calls()
    {
        var hits = ScanCode(
            Path.Combine(SrcRoot, "domain"),
            new Regex(@"Guid\.NewGuid\s*\("));

        Assert.True(hits.Count == 0,
            "Domain layer must not call Guid.NewGuid(). Use IIdGenerator. " +
            "Hits:\n" + string.Join("\n", hits));
    }

    [Fact]
    public void Domain_layer_contains_no_DateTime_UtcNow_calls()
    {
        var hits = ScanCode(
            Path.Combine(SrcRoot, "domain"),
            new Regex(@"DateTime(Offset)?\.UtcNow\b"));

        Assert.True(hits.Count == 0,
            "Domain layer must not read DateTime.UtcNow. Use IClock. " +
            "Hits:\n" + string.Join("\n", hits));
    }

    // ─────────────────────────────────────────────────────────────────────
    // §3 Dependency graph
    // ─────────────────────────────────────────────────────────────────────

    [Fact]
    public void Engines_do_not_reference_runtime_platform_or_infrastructure()
    {
        var hits = ScanCode(
            Path.Combine(SrcRoot, "engines"),
            new Regex(@"^\s*using\s+Whyce(space)?\.(Runtime|Platform|Infrastructure)\b",
                RegexOptions.Multiline));

        Assert.True(hits.Count == 0,
            "Engines must depend only on domain + shared contracts. Hits:\n"
            + string.Join("\n", hits));
    }

    [Fact]
    public void Projections_do_not_reference_runtime_directly()
    {
        var hits = ScanCode(
            Path.Combine(SrcRoot, "projections"),
            new Regex(@"^\s*using\s+Whyce(space)?\.Runtime\b", RegexOptions.Multiline));

        Assert.True(hits.Count == 0,
            "Projections must consume events via contracts only, not reference runtime. Hits:\n"
            + string.Join("\n", hits));
    }

    [Fact]
    public void Platform_non_composition_code_does_not_reference_domain_engines_or_projections()
    {
        // Sanctioned exemptions (user ruling 2026-04-08):
        //   • /platform/host/composition/** is the composition root and may
        //     reference any layer — wiring concrete types is its job.
        //   • Shared kernel primitives are dependency-free value types and
        //     are allowed across all layers.
        var compositionPrefix = Path.Combine(SrcRoot, "platform", "host", "composition")
            + Path.DirectorySeparatorChar;

        var hits = ScanCode(
                Path.Combine(SrcRoot, "platform"),
                new Regex(@"^\s*using\s+Whyce(space)?\.(Domain|Engines|Projections)\b",
                    RegexOptions.Multiline))
            .Where(line => !line.StartsWith(compositionPrefix, System.StringComparison.OrdinalIgnoreCase))
            .Where(line => !line.Contains("Whycespace.Domain.SharedKernel", System.StringComparison.OrdinalIgnoreCase))
            .ToList();

        Assert.True(hits.Count == 0,
            "Non-composition platform code must depend on runtime + adapters only " +
            "(composition roots and shared-kernel primitives are exempt). Hits:\n"
            + string.Join("\n", hits));
    }

    // ─────────────────────────────────────────────────────────────────────
    // §4 Engine purity — no DB/Kafka/Redis
    // ─────────────────────────────────────────────────────────────────────

    [Fact]
    public void Engines_do_not_use_database_kafka_or_redis_apis()
    {
        var hits = ScanCode(
            Path.Combine(SrcRoot, "engines"),
            new Regex(@"\b(DbContext|IDbConnection|NpgsqlConnection|IConnectionMultiplexer|StackExchange\.Redis|IProducer<|KafkaProducer|ProducerBuilder)\b"));

        Assert.True(hits.Count == 0,
            "Engines must be stateless: no DB, Kafka, or Redis. Use IEngineContext.EmitEvents. Hits:\n"
            + string.Join("\n", hits));
    }

    // ─────────────────────────────────────────────────────────────────────
    // §6 Event fabric hard lock — no direct Kafka publish outside outbox
    // ─────────────────────────────────────────────────────────────────────

    [Fact]
    public void No_direct_Kafka_publish_outside_outbox_publisher()
    {
        // Sanctioned publish sites (canonical whitelist, user ruling 2026-04-08).
        // The §6 "no direct Kafka publish" rule applies ONLY to domain-event
        // emission paths — not to DLQ handling, infrastructure factories, or
        // consumer-side recovery flows.
        //   1. KafkaOutboxPublisher.cs            — outbound domain events (S0/S0b)
        //   2. GenericKafkaProjectionConsumerWorker.cs — consumer-side DLQ
        //   3. InfrastructureComposition.cs       — producer factory / DI wiring
        var sanctioned = new[]
        {
            "KafkaOutboxPublisher.cs",
            "GenericKafkaProjectionConsumerWorker.cs",
            "InfrastructureComposition.cs",
        };

        var hits = ScanCode(
            SrcRoot,
            new Regex(@"\.(ProduceAsync|Produce)\s*\(|new\s+ProducerBuilder\b"))
            .Where(line => !sanctioned.Any(s => line.Contains(s, System.StringComparison.OrdinalIgnoreCase)))
            .ToList();

        Assert.True(hits.Count == 0,
            "Direct Kafka publish is forbidden outside KafkaOutboxPublisher. " +
            "All event emission must flow through IEventFabric → outbox. Hits:\n"
            + string.Join("\n", hits));
    }

    // ─────────────────────────────────────────────────────────────────────
    // H7a — Kafka partition key must be aggregate_id, never correlation_id
    // ─────────────────────────────────────────────────────────────────────

    [Fact]
    public void Event_store_adapter_enforces_expected_version_check()
    {
        var path = Path.Combine(SrcRoot, "platform", "host", "adapters", "PostgresEventStoreAdapter.cs");
        var content = File.ReadAllText(path);

        // The H8b enforcement block must be present. Pinning the canonical
        // form prevents a future refactor from silently reverting to the
        // pre-H8b "ignore parameter" pattern.
        Assert.Contains("expectedVersion != -1", content);
        Assert.Contains("ConcurrencyConflictException", content);
    }

    [Fact]
    public void Outbox_publisher_keys_kafka_messages_by_aggregate_id_not_correlation_id()
    {
        var path = Path.Combine(SrcRoot, "platform", "host", "adapters", "KafkaOutboxPublisher.cs");
        var content = File.ReadAllText(path);

        Assert.DoesNotContain("Key = entry.CorrelationId.ToString()", content);
        // Both the main publish path and the DLQ path must use aggregate id.
        var aggregateKeyOccurrences = Regex.Matches(content, @"Key\s*=\s*entry\.AggregateId\.ToString\(\)").Count;
        Assert.True(aggregateKeyOccurrences >= 2,
            $"KafkaOutboxPublisher must key BOTH the main publish path AND the DLQ path by AggregateId. " +
            $"Found {aggregateKeyOccurrences} occurrence(s) of `Key = entry.AggregateId.ToString()`.");
    }

    // ─────────────────────────────────────────────────────────────────────
    // §2 Projection layer — no projection HANDLERS in runtime
    // ─────────────────────────────────────────────────────────────────────
    // Note: dispatcher / registry / writer interface / rebuilder are
    // explicitly allowed in runtime per user ruling 2026-04-08.

    [Fact]
    public void Runtime_contains_no_projection_handler_classes()
    {
        var runtimeProjectionDir = Path.Combine(SrcRoot, "runtime", "projection");
        if (!Directory.Exists(runtimeProjectionDir))
            return;

        var handlerFiles = Directory.EnumerateFiles(runtimeProjectionDir, "*Handler.cs", SearchOption.AllDirectories)
            .Where(p => !p.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}"))
            .Where(p => !p.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"))
            .ToList();

        Assert.True(handlerFiles.Count == 0,
            "Runtime must not contain projection *Handler.cs files. They belong in src/projections/. Found:\n"
            + string.Join("\n", handlerFiles));
    }

    // ─────────────────────────────────────────────────────────────────────
    // helpers
    // ─────────────────────────────────────────────────────────────────────

    private static System.Collections.Generic.List<string> ScanCode(string root, Regex pattern)
    {
        var hits = new System.Collections.Generic.List<string>();
        if (!Directory.Exists(root)) return hits;

        foreach (var file in Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories))
        {
            // skip build output
            if (file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") ||
                file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"))
                continue;

            var lines = File.ReadAllLines(file);
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                var stripped = StripCommentAndString(line);
                if (pattern.IsMatch(stripped))
                    hits.Add($"{file}:{i + 1}: {line.Trim()}");
            }
        }
        return hits;
    }

    /// <summary>
    /// Removes // line comments and naive string literals so doc comments and
    /// string-embedded mentions of forbidden APIs do not produce false positives.
    /// Block comments (/* */) are intentionally not handled — none of the
    /// scanned APIs appear in block comments in this repo. Add handling if that
    /// changes.
    /// </summary>
    private static string StripCommentAndString(string line)
    {
        var commentIdx = line.IndexOf("//", System.StringComparison.Ordinal);
        if (commentIdx >= 0) line = line.Substring(0, commentIdx);
        // crude string strip: remove anything between double quotes
        return Regex.Replace(line, "\"[^\"]*\"", "\"\"");
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            if (Directory.Exists(Path.Combine(dir.FullName, "src")) &&
                Directory.Exists(Path.Combine(dir.FullName, "claude")))
                return dir.FullName;
            dir = dir.Parent;
        }
        throw new DirectoryNotFoundException("Could not locate WBSM repo root from " + AppContext.BaseDirectory);
    }
}
