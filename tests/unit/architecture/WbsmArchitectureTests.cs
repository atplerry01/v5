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

    // ─────────────────────────────────────────────────────────────────────
    // DET-SEED-DERIVATION-01 (phase1.6-S1.1)
    // ─────────────────────────────────────────────────────────────────────
    // IIdGenerator.Generate(seed) is the canonical deterministic-id seam.
    // The engine is deterministic only as far as its seed is — if the seed
    // string embeds wall-clock ticks, Guid.NewGuid, Random, or any other
    // entropy source, the entire mechanism is defeated and IDs diverge on
    // replay.
    //
    // This test scans the entire src/ tree for any line that calls
    // .Generate( and contains a forbidden token in the same statement. It
    // intentionally does NOT use StripCommentAndString — the forbidden
    // tokens we're hunting are themselves inside string interpolations,
    // and stripping the strings would hide every real violation.
    //
    // Closes the regression vector that produced the phase-1 audit S1.1.
    [Fact]
    public void IdGenerator_Generate_seeds_contain_no_clock_or_random_entropy()
    {
        var forbidden = new Regex(
            @"\.Generate\s*\([^)]*?(?:" +
            @"\.UtcNow|\.Now\b|" +     // IClock.UtcNow, DateTime.Now/UtcNow
            @"\.Ticks\b|" +             // any *.Ticks read
            @"Guid\.NewGuid|" +         // Guid.NewGuid()
            @"\bnew\s+Random\b|" +      // new Random(...)
            @"RandomNumberGenerator|" + // crypto RNG
            @"Stopwatch\." +            // Stopwatch.GetTimestamp / .ElapsedTicks
            @")");

        var hits = new System.Collections.Generic.List<string>();
        foreach (var file in Directory.EnumerateFiles(SrcRoot, "*.cs", SearchOption.AllDirectories))
        {
            if (file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") ||
                file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"))
                continue;

            var lines = File.ReadAllLines(file);
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                // skip line comments only — keep string literals intact
                var commentIdx = line.IndexOf("//", System.StringComparison.Ordinal);
                var scanned = commentIdx >= 0 ? line.Substring(0, commentIdx) : line;
                if (forbidden.IsMatch(scanned))
                    hits.Add($"{file}:{i + 1}: {line.Trim()}");
            }
        }

        Assert.True(hits.Count == 0,
            "DET-SEED-DERIVATION-01: IIdGenerator.Generate(seed) seeds must " +
            "derive only from stable command coordinates. Forbidden tokens " +
            "(clock ticks, Guid.NewGuid, Random, Stopwatch) found in seed " +
            "construction:\n" + string.Join("\n", hits));
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
    // CTX-REPLAY-RESET-01 (phase1.6-S1.4)
    // ─────────────────────────────────────────────────────────────────────
    // CommandContext exposes an internal replay reset seam
    // (EnableReplayMode / ResetForReplay / DisableReplayMode / IsReplayMode)
    // intended for a future deterministic replay tool. There is no
    // production caller today; the seam is exposed only to the unit test
    // assembly via InternalsVisibleTo. This test scans all of src/ and
    // asserts that no production code under src/ references any of the
    // four members.
    //
    // When a production caller is eventually added (a separate, explicit
    // gate), the engineer adding it MUST extend this test with a tightly
    // scoped allowlist of file paths AND update the InternalsVisibleTo
    // declaration in src/shared/Whycespace.Shared.csproj. Both changes
    // are required — adding the IVT alone (or adding a caller without
    // updating the allowlist) will fail this invariant.

    [Fact]
    public void CommandContext_replay_reset_seam_has_no_production_callers()
    {
        // Allowlist: production files permitted to call the seam. Empty
        // today by design. Update both this list AND the IVT in
        // Whycespace.Shared.csproj when adding a real caller.
        var allowlist = System.Array.Empty<string>();

        var pattern = new Regex(
            @"\b(ResetForReplay|EnableReplayMode|DisableReplayMode|IsReplayMode)\b");

        var hits = ScanCode(SrcRoot, pattern)
            // Exclude the CommandContext source file itself — it OWNS the
            // seam, it isn't a caller. The reflection check below pins
            // that the members exist on the right type.
            .Where(line => !line.Contains(
                Path.Combine("shared", "contracts", "runtime", "CommandContext.cs"),
                System.StringComparison.OrdinalIgnoreCase))
            .Where(line => !allowlist.Any(a =>
                line.Contains(a, System.StringComparison.OrdinalIgnoreCase)))
            .ToList();

        Assert.True(hits.Count == 0,
            "CommandContext replay reset seam has no allowlisted production " +
            "callers. To add one, extend the allowlist in this test AND " +
            "verify the IVT in src/shared/Whycespace.Shared.csproj is " +
            "scoped only to assemblies that need it. (phase1.6-S1.4 / " +
            "CTX-REPLAY-RESET-01) Hits:\n" + string.Join("\n", hits));
    }

    [Fact]
    public void CommandContext_replay_reset_seam_members_are_internal()
    {
        // Reflection check that pins the access modifier of every seam
        // member. If a future refactor accidentally promotes any of these
        // to public, this test fails immediately — the seam must never
        // leak past the IVT boundary.
        var ctxType = typeof(Whyce.Shared.Contracts.Runtime.CommandContext);
        var members = new[]
        {
            "EnableReplayMode",
            "DisableReplayMode",
            "ResetForReplay",
            "get_IsReplayMode",
        };

        foreach (var name in members)
        {
            var method = ctxType.GetMethod(
                name,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(method);
            Assert.False(method!.IsPublic,
                $"CommandContext.{name} must be internal, not public. " +
                "Replay reset seam must not leak past the IVT boundary. " +
                "(phase1.6-S1.4 / CTX-REPLAY-RESET-01)");
            Assert.True(method.IsAssembly,
                $"CommandContext.{name} must be internal (IsAssembly == true). " +
                "Replay reset seam must not be exposed to derived types or " +
                "via protected access. (phase1.6-S1.4 / CTX-REPLAY-RESET-01)");
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // E-LIFECYCLE-FACTORY-CALL-SITE-01 (phase1.6-S1.2)
    // ─────────────────────────────────────────────────────────────────────
    // T1M engines must not invoke aggregate command methods directly. The
    // canonical lifecycle transition factory is WorkflowLifecycleEventFactory;
    // it is the only allowed construction site for lifecycle events. The
    // aggregate's previous public Resume() command method has been removed
    // — state change happens only via Apply on replay.
    //
    // We pin two complementary invariants here so a regression cannot pass:
    //   1. Reflection: WorkflowExecutionAggregate has no public Resume method
    //   2. Source scan: nothing under src/engines/** invokes a literal
    //      .Resume( on a workflow aggregate variable.

    [Fact]
    public void WorkflowExecutionAggregate_does_not_expose_a_public_Resume_command()
    {
        var aggregateType = typeof(Whycespace.Domain.OrchestrationSystem.Workflow.Execution.WorkflowExecutionAggregate);
        var publicResume = aggregateType.GetMethods(
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
            .FirstOrDefault(m => m.Name == "Resume");

        Assert.True(publicResume is null,
            "WorkflowExecutionAggregate must not expose a public Resume() command. " +
            "Lifecycle resume flows through WorkflowLifecycleEventFactory.Resumed; " +
            "state change happens only via Apply on replay. " +
            "(phase1.6-S1.2 / E-LIFECYCLE-FACTORY-CALL-SITE-01)");
    }

    [Fact]
    public void Engines_do_not_call_Resume_on_workflow_aggregate_directly()
    {
        // Source scan covering src/engines/**. The previous violation lived
        // at WorkflowExecutionReplayService.cs:111 as `aggregate.Resume();`.
        // After the S1.2 fix the only legitimate construction path is
        // WorkflowLifecycleEventFactory.Resumed(aggregate). Any direct
        // .Resume( call on a workflow-execution variable in the engine
        // layer is a regression.
        var hits = ScanCode(
            Path.Combine(SrcRoot, "engines"),
            new Regex(@"\b(workflowExecution|aggregate)\.Resume\s*\(",
                RegexOptions.IgnoreCase));

        Assert.True(hits.Count == 0,
            "T1M engines must not call .Resume() on a workflow aggregate. " +
            "Use WorkflowLifecycleEventFactory.Resumed(aggregate) instead. Hits:\n"
            + string.Join("\n", hits));
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

    // ─────────────────────────────────────────────────────────────────────
    // API edge hardening — ConcurrencyConflictException must travel
    // untouched from the event store to the platform/api middleware
    // ─────────────────────────────────────────────────────────────────────

    [Fact]
    public void Concurrency_conflict_exception_handler_exists_and_targets_the_right_type()
    {
        var handlerPath = Path.Combine(SrcRoot, "platform", "api", "middleware",
            "ConcurrencyConflictExceptionHandler.cs");
        Assert.True(File.Exists(handlerPath),
            "ConcurrencyConflictExceptionHandler.cs must exist at " + handlerPath);

        var content = File.ReadAllText(handlerPath);
        Assert.Contains("IExceptionHandler", content);
        Assert.Contains("ConcurrencyConflictException", content);
        Assert.Contains("urn:whyce:error:concurrency-conflict", content);
        Assert.Contains("Status409Conflict", content);
    }

    [Fact]
    public void No_upstream_layer_catches_ConcurrencyConflictException()
    {
        // The exception must travel untouched from PostgresEventStoreAdapter
        // through runtime / engines / projections / non-middleware platform
        // to the IExceptionHandler in src/platform/api/middleware/. Any catch
        // upstream silently breaks the H8b -> 409 mapping.
        var roots = new[]
        {
            Path.Combine(SrcRoot, "runtime"),
            Path.Combine(SrcRoot, "engines"),
            Path.Combine(SrcRoot, "projections"),
        };

        var hits = new System.Collections.Generic.List<string>();
        foreach (var root in roots)
        {
            hits.AddRange(ScanCode(root,
                new Regex(@"catch\s*\(\s*ConcurrencyConflictException\b")));
            hits.AddRange(ScanCode(root,
                new Regex(@"\bis\s+ConcurrencyConflictException\b")));
        }

        // Same scan over src/platform EXCLUDING the middleware directory.
        var middlewarePrefix = Path.Combine(SrcRoot, "platform", "api", "middleware")
            + Path.DirectorySeparatorChar;
        var platformHits = ScanCode(Path.Combine(SrcRoot, "platform"),
                new Regex(@"catch\s*\(\s*ConcurrencyConflictException\b|\bis\s+ConcurrencyConflictException\b"))
            .Where(line => !line.StartsWith(middlewarePrefix, System.StringComparison.OrdinalIgnoreCase))
            .ToList();
        hits.AddRange(platformHits);

        Assert.True(hits.Count == 0,
            "ConcurrencyConflictException must travel untouched from the event store " +
            "to the API middleware. Catches found:\n" + string.Join("\n", hits));
    }

    // ─────────────────────────────────────────────────────────────────────
    // Projection hardening — workflow read model immutability + replay safety
    // ─────────────────────────────────────────────────────────────────────

    [Fact]
    public void Workflow_read_model_step_outputs_is_read_only_dictionary()
    {
        var path = Path.Combine(SrcRoot, "shared", "contracts", "projections",
            "orchestration-system", "workflow", "WorkflowExecutionReadModel.cs");
        var content = File.ReadAllText(path);

        Assert.Contains("IReadOnlyDictionary<string, object?> StepOutputs", content);
        // Must NOT regress to mutable Dictionary<,> on the property declaration.
        Assert.DoesNotContain("public Dictionary<string, object?> StepOutputs", content);
    }

    [Fact]
    public void Workflow_projection_handler_does_not_mutate_step_outputs_in_place()
    {
        var path = Path.Combine(SrcRoot, "projections", "orchestration-system",
            "workflow", "handler", "WorkflowExecutionProjectionHandler.cs");
        var content = File.ReadAllText(path);

        // Forbid the mutation pattern `existing.StepOutputs[...] = ...`.
        Assert.DoesNotContain(".StepOutputs[", content);
    }

    [Fact]
    public void Workflow_projection_handler_is_replay_safe_log_and_skip()
    {
        var path = Path.Combine(SrcRoot, "projections", "orchestration-system",
            "workflow", "handler", "WorkflowExecutionProjectionHandler.cs");
        var content = File.ReadAllText(path);

        // The pre-hardening pattern threw on missing prior state. Forbid it
        // ever returning by pinning the absence of the throw form.
        Assert.DoesNotContain("?? throw new InvalidOperationException(", content);
    }

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
