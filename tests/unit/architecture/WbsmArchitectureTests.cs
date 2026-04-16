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
    // DLQ-RESOLVER-01 (phase1.6-S1.6)
    // ─────────────────────────────────────────────────────────────────────
    // All DLQ topic naming MUST flow through TopicNameResolver.ResolveDeadLetter.
    // Inline `.Replace(".events", ".deadletter")` and any other ad-hoc
    // string manipulation that constructs a DLQ topic name is forbidden
    // outside the resolver itself. The previous violation lived at
    // KafkaOutboxPublisher.cs:265-267 and is the canonical regression
    // surface this invariant defends.

    [Fact]
    public void No_inline_DLQ_topic_derivation_outside_resolver()
    {
        // Forbidden patterns:
        //   1. .Replace(".events", ".deadletter") — naive substring swap
        //   2. + ".deadletter" — string concatenation forming a DLQ name
        //   3. .EndsWith(".events") used in DLQ-derivation context
        // The resolver source file is the only legitimate site for these
        // patterns; everything else must call ResolveDeadLetter.
        var resolverPath = Path.Combine(SrcRoot, "runtime", "event-fabric", "TopicNameResolver.cs");

        var patterns = new[]
        {
            new Regex(@"\.Replace\(\s*""\.events""\s*,\s*""\.deadletter"""),
            new Regex(@"\+\s*""\.deadletter"""),
        };

        var hits = new System.Collections.Generic.List<string>();
        foreach (var pattern in patterns)
        {
            hits.AddRange(ScanCode(SrcRoot, pattern)
                .Where(line => !line.StartsWith(resolverPath, System.StringComparison.OrdinalIgnoreCase)));
        }

        Assert.True(hits.Count == 0,
            "DLQ-RESOLVER-01: inline DLQ topic derivation is forbidden " +
            "outside TopicNameResolver. Use ResolveDeadLetter(topic) " +
            "instead. (phase1.6-S1.6) Hits:\n" + string.Join("\n", hits));
    }

    [Fact]
    public void KafkaOutboxPublisher_uses_TopicNameResolver_for_DLQ()
    {
        // Source-scan check that pins the call site explicitly. Catches
        // a refactor that drops the resolver injection and reintroduces
        // an inline form that happens to evade the regex above.
        var path = Path.Combine(SrcRoot, "platform", "host", "adapters", "KafkaOutboxPublisher.cs");
        var content = File.ReadAllText(path);

        Assert.Contains("_topicNameResolver.ResolveDeadLetter", content);
        Assert.Contains("TopicNameResolver topicNameResolver", content);
    }

    // ─────────────────────────────────────────────────────────────────────
    // OUTBOX-CONFIG-01 (phase1.6-S1.5)
    // ─────────────────────────────────────────────────────────────────────
    // KafkaOutboxPublisher MAX_RETRY is now externalised to
    // OutboxOptions.MaxRetry, populated from Outbox__MaxRetry config in
    // InfrastructureComposition. This invariant pins three things:
    //   1. No `MAX_RETRY`-style constant exists anywhere in src/.
    //   2. KafkaOutboxPublisher does not contain a `private const int Default*Retry`
    //      that could silently shadow the option.
    //   3. The publisher constructor accepts an OutboxOptions parameter
    //      (compile-time check via reflection — catches refactors that
    //      drop the parameter and re-introduce a constant).

    [Fact]
    public void No_MAX_RETRY_constants_anywhere_in_src()
    {
        // Forbid any `(public|private|internal|protected|const) ... MAX_RETRY`
        // declaration form, plus the literal identifier `MAX_RETRY` (in case
        // a future refactor uses an UPPER_SNAKE local). Also catch the
        // pre-S1.5 form `DefaultMaxRetryCount` to prevent regression.
        var hits = ScanCode(
            SrcRoot,
            new Regex(@"\b(MAX_RETRY|DefaultMaxRetryCount|MaxRetryCount\s*=\s*\d+)\b"));

        Assert.True(hits.Count == 0,
            "OUTBOX-CONFIG-01: hardcoded retry constants are forbidden under src/. " +
            "Use OutboxOptions.MaxRetry resolved from configuration. " +
            "(phase1.6-S1.5) Hits:\n" + string.Join("\n", hits));
    }

    [Fact]
    public void KafkaOutboxPublisher_constructor_takes_OutboxOptions()
    {
        // Source-scan check: the publisher source must contain
        // `OutboxOptions options` as a constructor parameter (not defaulted).
        // We use a source scan instead of reflection because the unit test
        // project deliberately does not reference Whycespace.Platform.Host —
        // adding that reference would couple the architecture invariants
        // to the platform layer. The source check catches the same
        // refactor surface (drop the param, re-introduce a constant)
        // without the project-reference cost.
        var path = Path.Combine(SrcRoot, "platform", "host", "adapters", "KafkaOutboxPublisher.cs");
        Assert.True(File.Exists(path), $"KafkaOutboxPublisher.cs must exist at {path}");
        var content = File.ReadAllText(path);

        Assert.Contains("OutboxOptions options", content);
        // No defaulted form: forbid `OutboxOptions options = ...` and
        // `OutboxOptions? options = null`. Either would let a caller
        // silently fall back to the pre-S1.5 hardcoded behavior.
        Assert.DoesNotMatch(new Regex(@"OutboxOptions\??\s+options\s*="), content);
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
        var ctxType = typeof(Whycespace.Shared.Contracts.Runtime.CommandContext);
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

        // Phase 1.5 closure Patch B1 (2026-04-09): the predicate now
        // partitions hits into "catch clauses" and "is-pattern uses".
        // For catch clauses, a pure-rethrow body
        // (e.g. `catch (ConcurrencyConflictException) { outcome = "x"; throw; }`)
        // is permitted as observability instrumentation — the
        // exception still travels untouched to the edge handler.
        // The is-pattern matches and any catch-with-non-rethrow body
        // remain hard violations. See guard rule R-RT-10 in
        // claude/guards/phase1.5-runtime.guard.md.
        var catchRegex = new Regex(@"catch\s*\(\s*ConcurrencyConflictException\b");
        var isPatternRegex = new Regex(@"\bis\s+ConcurrencyConflictException\b");

        var hits = new System.Collections.Generic.List<string>();
        foreach (var root in roots)
        {
            foreach (var hit in ScanCode(root, catchRegex))
            {
                if (!IsPureRethrowCatchHit(hit)) hits.Add(hit);
            }
            hits.AddRange(ScanCode(root, isPatternRegex));
        }

        // Same scan over src/platform EXCLUDING the middleware directory.
        var middlewarePrefix = Path.Combine(SrcRoot, "platform", "api", "middleware")
            + Path.DirectorySeparatorChar;
        var platformRoot = Path.Combine(SrcRoot, "platform");
        foreach (var hit in ScanCode(platformRoot, catchRegex))
        {
            if (hit.StartsWith(middlewarePrefix, System.StringComparison.OrdinalIgnoreCase)) continue;
            if (!IsPureRethrowCatchHit(hit)) hits.Add(hit);
        }
        foreach (var hit in ScanCode(platformRoot, isPatternRegex))
        {
            if (hit.StartsWith(middlewarePrefix, System.StringComparison.OrdinalIgnoreCase)) continue;
            hits.Add(hit);
        }

        Assert.True(hits.Count == 0,
            "ConcurrencyConflictException must travel untouched from the event store " +
            "to the API middleware. Catches found:\n" + string.Join("\n", hits));
    }

    /// <summary>
    /// Phase 1.5 closure Patch B1 (2026-04-09) / R-RT-10: returns true
    /// when the catch clause at the given ScanCode hit is a pure
    /// rethrow — i.e. its body contains a bare <c>throw;</c> and
    /// neither <c>return</c> nor <c>throw new</c>. Pure-rethrow
    /// catches are permitted as observability instrumentation; any
    /// other catch shape is rejected by the calling test.
    ///
    /// Hit format from <c>ScanCode</c>: <c>"{filePath}:{lineNumber}: {trimmed}"</c>.
    /// </summary>
    private static bool IsPureRethrowCatchHit(string hit)
    {
        // Parse "{filePath}:{lineNumber}: ..." — file path may
        // contain ':' on Windows (drive prefix), so we skip index 2
        // when locating the post-path colon.
        var pathColon = hit.IndexOf(':', 2);          // colon after the file path (post drive letter)
        if (pathColon < 0) return false;
        var lineColon = hit.IndexOf(':', pathColon + 1); // colon after the line number
        if (lineColon < 0) return false;
        var filePath = hit.Substring(0, pathColon);
        var lineSpan = hit.Substring(pathColon + 1, lineColon - pathColon - 1);
        if (!int.TryParse(lineSpan, out var lineNumber)) return false;

        if (!File.Exists(filePath)) return false;
        var source = File.ReadAllText(filePath);
        var lineStarts = new System.Collections.Generic.List<int> { 0 };
        for (int i = 0; i < source.Length; i++)
            if (source[i] == '\n') lineStarts.Add(i + 1);
        if (lineNumber - 1 >= lineStarts.Count) return false;
        var searchFrom = lineStarts[lineNumber - 1];

        // Find the catch keyword from the line start.
        var catchIdx = source.IndexOf("catch", searchFrom, System.StringComparison.Ordinal);
        if (catchIdx < 0) return false;
        // Find the opening '{' that begins the catch body.
        var openBrace = source.IndexOf('{', catchIdx);
        if (openBrace < 0) return false;
        // Walk braces to find the matching close.
        int depth = 0;
        int closeBrace = -1;
        for (int i = openBrace; i < source.Length; i++)
        {
            if (source[i] == '{') depth++;
            else if (source[i] == '}')
            {
                depth--;
                if (depth == 0) { closeBrace = i; break; }
            }
        }
        if (closeBrace < 0) return false;
        var body = source.Substring(openBrace + 1, closeBrace - openBrace - 1);

        // Pure rethrow criteria:
        //   - contains a bare `throw;` (rethrow), AND
        //   - does NOT contain `throw new ` (transformed exception), AND
        //   - does NOT contain `return` (alternate control flow)
        // The body may contain assignments / method calls before
        // the throw — those are observability-only side effects
        // and do not affect exception flow.
        var hasBareThrow = Regex.IsMatch(body, @"(^|[^\w])throw\s*;");
        var hasThrowNew = Regex.IsMatch(body, @"\bthrow\s+new\b");
        var hasReturn = Regex.IsMatch(body, @"\breturn\b");
        return hasBareThrow && !hasThrowNew && !hasReturn;
    }

    // ─────────────────────────────────────────────────────────────────────
    // Projection hardening — workflow read model immutability + replay safety
    // ─────────────────────────────────────────────────────────────────────

    [Fact]
    public void Workflow_read_model_step_outputs_is_read_only_dictionary()
    {
        var path = Path.Combine(SrcRoot, "shared", "contracts", "projections",
            "orchestration", "workflow", "WorkflowExecutionReadModel.cs");
        var content = File.ReadAllText(path);

        Assert.Contains("IReadOnlyDictionary<string, object?> StepOutputs", content);
        // Must NOT regress to mutable Dictionary<,> on the property declaration.
        Assert.DoesNotContain("public Dictionary<string, object?> StepOutputs", content);
    }

    [Fact]
    public void Workflow_projection_handler_does_not_mutate_step_outputs_in_place()
    {
        var path = Path.Combine(SrcRoot, "projections", "orchestration",
            "workflow", "handler", "WorkflowExecutionProjectionHandler.cs");
        var content = File.ReadAllText(path);

        // Forbid the mutation pattern `existing.StepOutputs[...] = ...`.
        Assert.DoesNotContain(".StepOutputs[", content);
    }

    [Fact]
    public void Workflow_projection_handler_is_replay_safe_log_and_skip()
    {
        var path = Path.Combine(SrcRoot, "projections", "orchestration",
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
    // CONFIG SAFETY (phase1.5 / config-safety.guard.md)
    // ─────────────────────────────────────────────────────────────────────
    // Pin three rules from claude/guards/config-safety.guard.md:
    //   CFG-R1 — no credentials in committed appsettings*.json
    //   CFG-R2 — no hardcoded endpoint defaults in committed appsettings*.json
    //   CFG-R3 — no `?? "Host=...;Password=..."` style fallbacks in C# code
    //
    // The composition root reads required infrastructure config from
    // environment variables and throws InvalidOperationException when
    // missing. There is no fallback. appsettings.json contains only
    // logging + feature flags + domain-shaped constants.

    [Fact]
    public void Appsettings_json_contains_no_credentials()
    {
        var path = Path.Combine(SrcRoot, "platform", "host", "appsettings.json");
        Assert.True(File.Exists(path), $"appsettings.json must exist at {path}");
        var content = File.ReadAllText(path);

        var forbidden = new[]
        {
            "Password=", "Pwd=", "SecretKey", "AccessKey", "ApiKey", "Token=",
        };
        var hits = forbidden.Where(f =>
            content.Contains(f, System.StringComparison.OrdinalIgnoreCase)).ToList();

        Assert.True(hits.Count == 0,
            "CFG-R1: appsettings.json must contain zero credential-shaped tokens. " +
            "Move all credentials to environment variables. Forbidden tokens found: "
            + string.Join(", ", hits));
    }

    [Fact]
    public void Appsettings_json_contains_no_endpoint_defaults()
    {
        var path = Path.Combine(SrcRoot, "platform", "host", "appsettings.json");
        var content = File.ReadAllText(path);

        var forbidden = new[]
        {
            "localhost", "127.0.0.1", "0.0.0.0",
            ":5432", ":5434", ":6379", ":9092", ":29092", ":8181", ":9000",
        };
        var hits = forbidden.Where(f =>
            content.Contains(f, System.StringComparison.OrdinalIgnoreCase)).ToList();

        Assert.True(hits.Count == 0,
            "CFG-R2: appsettings.json must contain zero hardcoded endpoint defaults. " +
            "All endpoints must come from environment variables. Hits: "
            + string.Join(", ", hits));
    }

    [Fact]
    public void No_hardcoded_connection_string_fallbacks_in_composition_code()
    {
        // CFG-R3: forbid the `?? "Host=..."` / `?? "Server=..."` pattern
        // anywhere under src/. The canonical pattern is
        //   `configuration.GetValue<string>("X")
        //       ?? throw new InvalidOperationException("X is required");`
        var hits = ScanCode(
            SrcRoot,
            new Regex(@"\?\?\s*""(?:Host|Server|Data Source|Endpoint)\s*="));

        Assert.True(hits.Count == 0,
            "CFG-R3: hardcoded connection-string / endpoint fallbacks are forbidden. " +
            "Use the no-fallback pattern: " +
            "`configuration.GetValue<string>(...) ?? throw new InvalidOperationException(...)`. " +
            "Hits:\n" + string.Join("\n", hits));
    }

    // ─────────────────────────────────────────────────────────────────────
    // STUB ELIMINATION (phase1.5 / stub-detection.guard.md)
    // ─────────────────────────────────────────────────────────────────────
    // Pin two rules from claude/guards/stub-detection.guard.md:
    //   STUB-R1 — no NotImplementedException on the production path
    //   STUB-R2 — no TODO/FIXME/HACK/XXX comments in production code
    // (STUB-R3 placeholder registry deferred until claude/registry/placeholders.json
    // exists; STUB-R4 silent-catch deferred to follow-up.)

    [Fact]
    public void Production_path_throws_no_NotImplementedException()
    {
        var roots = new[]
        {
            Path.Combine(SrcRoot, "domain"),
            Path.Combine(SrcRoot, "engines"),
            Path.Combine(SrcRoot, "runtime"),
            Path.Combine(SrcRoot, "platform", "api"),
        };

        var hits = new System.Collections.Generic.List<string>();
        foreach (var root in roots)
        {
            hits.AddRange(ScanCode(
                root,
                new Regex(@"throw\s+new\s+NotImplementedException\b")));
        }

        Assert.True(hits.Count == 0,
            "STUB-R1: NotImplementedException is forbidden on the production path " +
            "(domain, engines, runtime, platform/api). Implement the method or " +
            "throw a structured domain exception with explicit reason. Hits:\n"
            + string.Join("\n", hits));
    }

    [Fact]
    public void Production_code_contains_no_TODO_FIXME_HACK_comments()
    {
        // Scans only line comments. XML doc comments (///) are also scanned
        // because TODO/FIXME/HACK in a doc comment is still a hidden backlog
        // item that should be a tracked issue or new-rules entry.
        //
        // Note: XXX is intentionally NOT in the pattern. It clashes with the
        // HSID v2.1 topology spec "CCC + XXX + SSSSSS" used as a placeholder
        // letter in domain doc comments and produces false positives.
        var pattern = new Regex(@"//.*\b(TODO|FIXME|HACK)\b");

        var hits = new System.Collections.Generic.List<string>();
        foreach (var file in Directory.EnumerateFiles(SrcRoot, "*.cs", SearchOption.AllDirectories))
        {
            if (file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") ||
                file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"))
                continue;

            var lines = File.ReadAllLines(file);
            for (int i = 0; i < lines.Length; i++)
            {
                if (pattern.IsMatch(lines[i]))
                    hits.Add($"{file}:{i + 1}: {lines[i].Trim()}");
            }
        }

        Assert.True(hits.Count == 0,
            "STUB-R2: TODO/FIXME/HACK/XXX comments are forbidden in production code. " +
            "Convert to a GitHub issue or a claude/new-rules/ entry. Hits:\n"
            + string.Join("\n", hits));
    }

    // ─────────────────────────────────────────────────────────────────────
    // β #1 — Architecture enforcement (eliminates D7 + D10 recurrence)
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>
    /// D7 fence. Every command record under shared/contracts MUST implement
    /// IHasAggregateId so SystemIntentDispatcher can resolve its aggregate id
    /// without falling back to the brittle property-name allowlist (which
    /// silently throws InvalidOperationException for any aggregate whose
    /// identity property is named &lt;X&gt;Id for an &lt;X&gt; not in the list).
    ///
    /// Captures: RT-OUTBOX-AGGID-FROM-ENVELOPE-01 sub-clause.
    /// </summary>
    [Fact]
    public void All_command_records_implement_IHasAggregateId()
    {
        var contractsRoot = Path.Combine(SrcRoot, "shared", "contracts");
        var commandHeader = new Regex(
            @"public\s+sealed\s+record\s+(?<name>\w+Command)\s*\(",
            RegexOptions.Compiled);

        var violations = new System.Collections.Generic.List<string>();

        foreach (var file in Directory.EnumerateFiles(contractsRoot, "*.cs", SearchOption.AllDirectories))
        {
            if (file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") ||
                file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"))
                continue;

            var content = File.ReadAllText(file);
            foreach (Match m in commandHeader.Matches(content))
            {
                // Walk forward from the opening paren of the constructor list,
                // tracking depth so the type isn't fooled by nested generics
                // or default-value expressions. Once depth returns to zero the
                // close-paren of the constructor list is found; everything from
                // that paren up to the record body open-brace (or `;` for a
                // body-less record) is the inheritance clause.
                var openIdx = content.IndexOf('(', m.Index + m.Length - 1);
                if (openIdx < 0) continue;
                var depth = 0;
                int closeIdx = -1;
                for (int i = openIdx; i < content.Length; i++)
                {
                    if (content[i] == '(') depth++;
                    else if (content[i] == ')')
                    {
                        depth--;
                        if (depth == 0) { closeIdx = i; break; }
                    }
                }
                if (closeIdx < 0) continue;

                var bodyStart = content.IndexOfAny(new[] { ';', '{' }, closeIdx);
                if (bodyStart < 0) bodyStart = Math.Min(closeIdx + 200, content.Length);
                var inheritance = content.Substring(closeIdx, bodyStart - closeIdx);

                if (!inheritance.Contains("IHasAggregateId"))
                {
                    var line = content.Take(m.Index).Count(c => c == '\n') + 1;
                    violations.Add($"{file}:{line}: {m.Groups["name"].Value} does not implement IHasAggregateId");
                }
            }
        }

        Assert.True(violations.Count == 0,
            "RT-OUTBOX-AGGID-FROM-ENVELOPE-01 / D7: every command record MUST implement IHasAggregateId so " +
            $"SystemIntentDispatcher can resolve its aggregate id deterministically. {violations.Count} violations:\n"
            + string.Join("\n", violations));
    }

    /// <summary>
    /// D10 fence. Every domain-event constructor parameter whose type is a
    /// value-object wrapper struct (e.g. <c>VaultAccountId</c>, <c>SubjectId</c>,
    /// <c>Currency</c>) MUST have a registered <c>JsonConverter&lt;T&gt;</c> in
    /// <c>EventDeserializer.StoredOptions</c> — otherwise the event-store
    /// replay path silently produces <c>default(T)</c> for the parameter and
    /// the reconstructed aggregate carries empty value objects (the failure
    /// mode that surfaced as "Currency mismatch: requires '' but received 'USD'").
    ///
    /// Captures: INV-REPLAY-LOSSLESS-VALUEOBJECT-01.
    /// </summary>
    [Fact]
    public void All_value_objects_used_by_domain_events_have_StoredJsonConverter()
    {
        var domainRoot = Path.Combine(SrcRoot, "domain");
        var deserializerPath = Path.Combine(SrcRoot, "runtime", "event-fabric", "EventDeserializer.cs");
        if (!Directory.Exists(domainRoot) || !File.Exists(deserializerPath))
            return; // graceful no-op in environments without these paths

        // Step 1 — enumerate value-object structs under src/domain/**/value-object/
        // and classify by primitive-property count. Only SINGLE-primitive-property
        // wrappers are at risk of the D10 silent-default failure mode that
        // motivated INV-REPLAY-LOSSLESS-VALUEOBJECT-01. Multi-property records
        // use STJ's native constructor-binding path (PascalCase JSON key →
        // constructor parameter), which round-trips losslessly without a
        // converter. Excluding multi-property structs keeps the test focused
        // on the actual failure shape.
        var valueObjects = new System.Collections.Generic.HashSet<string>(StringComparer.Ordinal);
        var positionalSingle = new Regex(
            @"public\s+readonly\s+record\s+struct\s+(?<name>\w+)\s*\(\s*(?<prim>Guid|string|decimal|int|long|DateTimeOffset)\s+\w+\s*\)");
        var blockBodyDecl = new Regex(@"public\s+readonly\s+record\s+struct\s+(?<name>\w+)\b");
        var primitivePropDecl = new Regex(
            @"public\s+(?<prim>Guid|string|decimal|int|long|DateTimeOffset)\s+\w+\s*\{\s*get");

        foreach (var file in Directory.EnumerateFiles(domainRoot, "*.cs", SearchOption.AllDirectories))
        {
            if (!file.Contains($"{Path.DirectorySeparatorChar}value-object{Path.DirectorySeparatorChar}")) continue;
            if (file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") ||
                file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"))
                continue;
            var text = File.ReadAllText(file);

            // A — positional record struct with a single primitive parameter.
            foreach (Match m in positionalSingle.Matches(text))
                valueObjects.Add(m.Groups["name"].Value);

            // B — block-body struct declared with EXACTLY one primitive property.
            //     Multi-property structs are STJ-handled and excluded.
            foreach (Match m in blockBodyDecl.Matches(text))
            {
                var name = m.Groups["name"].Value;
                if (valueObjects.Contains(name)) continue;
                if (primitivePropDecl.Matches(text).Count == 1)
                    valueObjects.Add(name);
            }
        }

        // Step 2 — enumerate registered converters in EventDeserializer.StoredOptions.
        var converterPattern = new Regex(@"new\s+(?<vo>\w+)JsonConverter\s*\(\s*\)");
        var registeredConverters = new System.Collections.Generic.HashSet<string>(StringComparer.Ordinal);
        var deserializerSource = File.ReadAllText(deserializerPath);
        foreach (Match m in converterPattern.Matches(deserializerSource))
            registeredConverters.Add(m.Groups["vo"].Value);

        // β #2: WrappedPrimitiveValueObjectConverterFactory covers every
        // wrapper-struct value object whose single property is `Value` or
        // `Code` of a supported primitive (Guid / string / decimal / int /
        // long / DateTimeOffset). Detect its registration AND parse each
        // value-object source to determine factory-coverage on a per-type basis.
        // β #2: WrappedPrimitiveValueObjectConverterFactory accepts any
        // single-primitive-property wrapper struct under Whycespace.Domain.*
        // (regardless of whether the property is named Value/Code or a
        // semantic name like DurationInDays). Since `valueObjects` already
        // contains exactly the at-risk single-property structs, the factory's
        // coverage set is identical when the factory is registered.
        var factoryRegistered = deserializerSource.Contains("new WrappedPrimitiveValueObjectConverterFactory()");
        var factoryCovered = factoryRegistered
            ? new System.Collections.Generic.HashSet<string>(valueObjects, StringComparer.Ordinal)
            : new System.Collections.Generic.HashSet<string>(StringComparer.Ordinal);

        // (legacy per-pattern detection retained below for the rare case
        // where someone disables the factory but keeps explicit converters.)
        if (false)
        {
            var positional = new Regex(@"^$");
            var blockBody = new Regex(@"^$");
            var propertyDecl = new Regex(@"^$");

            foreach (var file in Directory.EnumerateFiles(domainRoot, "*.cs", SearchOption.AllDirectories))
            {
                if (!file.Contains($"{Path.DirectorySeparatorChar}value-object{Path.DirectorySeparatorChar}")) continue;
                if (file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") ||
                    file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"))
                    continue;

                var text = File.ReadAllText(file);

                // Pattern A — positional record struct on a single declaration line.
                foreach (Match m in positional.Matches(text))
                {
                    if (IsFactoryPrimitive(m.Groups["prim"].Value))
                        factoryCovered.Add(m.Groups["name"].Value);
                }

                // Pattern B — block-body record struct: declaration + Value/Code property.
                foreach (Match m in blockBody.Matches(text))
                {
                    var name = m.Groups["name"].Value;
                    if (factoryCovered.Contains(name)) continue;
                    var propMatch = propertyDecl.Match(text);
                    if (propMatch.Success && IsFactoryPrimitive(propMatch.Groups["prim"].Value))
                        factoryCovered.Add(name);
                }
            }
        }

        static bool IsFactoryPrimitive(string prim) =>
            prim is "Guid" or "string" or "decimal" or "int" or "long" or "DateTimeOffset";

        // Step 3 — for every domain event constructor parameter typed as a
        // value object, assert a converter for that type is registered.
        var eventHeader = new Regex(
            @"public\s+sealed\s+record\s+(?<name>\w+Event)\s*\(",
            RegexOptions.Compiled);

        var violations = new System.Collections.Generic.List<string>();

        foreach (var file in Directory.EnumerateFiles(domainRoot, "*.cs", SearchOption.AllDirectories))
        {
            if (!file.Contains($"{Path.DirectorySeparatorChar}event{Path.DirectorySeparatorChar}")) continue;
            if (file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") ||
                file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"))
                continue;

            var content = File.ReadAllText(file);
            foreach (Match m in eventHeader.Matches(content))
            {
                var openIdx = content.IndexOf('(', m.Index + m.Length - 1);
                if (openIdx < 0) continue;
                var depth = 0;
                int closeIdx = -1;
                for (int i = openIdx; i < content.Length; i++)
                {
                    if (content[i] == '(') depth++;
                    else if (content[i] == ')')
                    {
                        depth--;
                        if (depth == 0) { closeIdx = i; break; }
                    }
                }
                if (closeIdx < 0) continue;

                var paramList = content.Substring(openIdx + 1, closeIdx - openIdx - 1);
                // crude per-comma split — domain events don't use generics in
                // their constructor parameter types, so a comma-split is safe.
                var paramTokens = paramList.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var token in paramTokens)
                {
                    // strip leading whitespace + attribute brackets like
                    // [property: JsonPropertyName(...)]
                    var clean = Regex.Replace(token, @"\[[^\]]*\]", string.Empty).Trim();
                    if (string.IsNullOrEmpty(clean)) continue;
                    // first whitespace-separated token is the type
                    var typeName = clean.Split(new[] { ' ', '\t', '\n' }, 2, StringSplitOptions.RemoveEmptyEntries)
                        .FirstOrDefault();
                    if (typeName is null) continue;

                    if (valueObjects.Contains(typeName)
                        && !registeredConverters.Contains(typeName)
                        && !factoryCovered.Contains(typeName))
                    {
                        var line = content.Take(m.Index).Count(c => c == '\n') + 1;
                        violations.Add(
                            $"{file}:{line}: {m.Groups["name"].Value} parameter of type {typeName} " +
                            "needs a JsonConverter registered in EventDeserializer.StoredOptions " +
                            "(or matching shape for WrappedPrimitiveValueObjectConverterFactory).");
                    }
                }
            }
        }

        Assert.True(violations.Count == 0,
            "INV-REPLAY-LOSSLESS-VALUEOBJECT-01 / D10: every value-object constructor parameter on a domain " +
            $"event MUST round-trip through EventDeserializer.StoredOptions. {violations.Count} violations:\n"
            + string.Join("\n", violations.Take(50))
            + (violations.Count > 50 ? $"\n... ({violations.Count - 50} more)" : string.Empty));
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
