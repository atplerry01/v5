using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Whycespace.Tests.Unit.Domain;

/// <summary>
/// Regression lock for the Business-System enforcement pass (prompt
/// <c>20260421-004717-business-system-enforcement-pass</c>). These five tests
/// pin the gains of that pass against drift:
///
/// <list type="bullet">
///   <item><b>Phase 1 — Invariant Guarantee</b>: every aggregate in
///   <c>src/domain/business-system/**</c> declares
///   <c>private void EnsureInvariants()</c> AND the body asserts more than
///   just identity non-emptiness.</item>
///   <item><b>Phase 2 — Aggregate Purity</b>: aggregate files do not import
///   another aggregate's namespace. Shared-kernel imports
///   (<c>Shared.Reference</c>, <c>Shared.Time</c>, <c>Shared.Pricing</c>) are
///   the only cross-folder business-system imports permitted.</item>
///   <item><b>Phase 3 — Event Semantic</b>: every event file matches
///   <c>&lt;Entity&gt;&lt;Action&gt;Event.cs</c>, its record declaration
///   matches the same pattern, and the record parameter list contains no
///   <c>Guid *Id</c> / <c>string *Id</c> primitive references.</item>
///   <item><b>Phase 4 — Policy Purity</b>: classes under <c>policy/</c>
///   folders contain no repository/persistence references and expose no
///   <c>void</c> methods (policies return decisions, they do not mutate).</item>
///   <item><b>Phase 5 — Primitive-ID Lock</b>: zero <c>public (Guid|string)
///   *Id</c> properties AND zero <c>(Guid|string) *Id</c> parameters anywhere
///   under <c>src/domain/business-system/**</c>.</item>
/// </list>
///
/// Scans are text-based (regex over source on disk), matching the existing
/// <c>StructuralEnforcementArchTests</c> + <c>WbsmArchitectureTests</c> style.
/// No reflection, no Roslyn.
/// </summary>
public sealed class BusinessSystemEnforcementLockTests
{
    private static readonly string RepoRoot = FindRepoRoot();
    private static readonly string BusinessSystemRoot =
        Path.Combine(RepoRoot, "src", "domain", "business-system");

    // ─────────────────────────────────────────────────────────────────────
    // Phase 1 — EnsureInvariants must exist and be non-trivial
    // ─────────────────────────────────────────────────────────────────────

    private static readonly Regex EnsureInvariantsBody = new(
        @"private\s+void\s+EnsureInvariants\s*\(\s*\)\s*\{(?<body>[\s\S]*?)\n\s{4}\}",
        RegexOptions.Compiled);

    [Fact]
    public void Every_business_system_aggregate_declares_a_non_trivial_EnsureInvariants()
    {
        Assert.True(Directory.Exists(BusinessSystemRoot),
            $"Business-system root missing at {BusinessSystemRoot}.");

        var missing = new List<string>();
        var trivial = new List<string>();

        foreach (var path in EnumerateAggregateFiles())
        {
            var text = File.ReadAllText(path);
            var match = EnsureInvariantsBody.Match(text);

            if (!match.Success)
            {
                missing.Add(Path.GetRelativePath(RepoRoot, path));
                continue;
            }

            var body = match.Groups["body"].Value;
            var throwCount = Regex.Matches(body, @"\bthrow\b").Count;

            // "Trivial" == the body only enforces identity (one throw on Id).
            // Every business-system aggregate must enforce at least two
            // distinct rules (identity + at least one lifecycle / required-
            // relationship / status guard).
            if (throwCount < 2)
                trivial.Add(Path.GetRelativePath(RepoRoot, path));
        }

        Assert.True(missing.Count == 0,
            "BS-INV-PRESENT-01 violation: the following business-system " +
            "aggregate files do not declare `private void EnsureInvariants()`. " +
            "Every aggregate MUST fail fast on invariant violation at the end " +
            "of every command.\nOffenders:\n  - " + string.Join("\n  - ", missing));

        Assert.True(trivial.Count == 0,
            "BS-INV-NON-TRIVIAL-01 violation: the following business-system " +
            "aggregate files declare `EnsureInvariants()` with fewer than two " +
            "`throw` statements. Identity-only validation is insufficient — " +
            "every aggregate must also assert its required relationships, " +
            "lifecycle state, or state-transition preconditions.\nOffenders:\n  - " +
            string.Join("\n  - ", trivial));
    }

    // ─────────────────────────────────────────────────────────────────────
    // Phase 2 — Aggregates depend on typed IDs only, never on other
    // aggregate namespaces. Shared kernel (Shared.*) is the sole allowed
    // cross-folder import inside business-system.
    // ─────────────────────────────────────────────────────────────────────

    private static readonly Regex BusinessSystemUsing = new(
        @"^using\s+Whycespace\.Domain\.BusinessSystem\.(?<tail>[^;]+);",
        RegexOptions.Compiled | RegexOptions.Multiline);

    [Fact]
    public void Business_system_aggregates_import_only_shared_kernel_namespaces()
    {
        Assert.True(Directory.Exists(BusinessSystemRoot),
            $"Business-system root missing at {BusinessSystemRoot}.");

        var offenders = new List<string>();

        foreach (var path in EnumerateAggregateFiles())
        {
            var text = File.ReadAllText(path);
            foreach (Match match in BusinessSystemUsing.Matches(text))
            {
                var tail = match.Groups["tail"].Value.Trim();
                if (tail.StartsWith("Shared.", StringComparison.Ordinal))
                    continue;

                offenders.Add(
                    $"{Path.GetRelativePath(RepoRoot, path)}:  " +
                    $"using Whycespace.Domain.BusinessSystem.{tail};");
            }
        }

        Assert.True(offenders.Count == 0,
            "BS-AGG-PURITY-01 violation: business-system aggregates must " +
            "reference other aggregates only via typed IDs (locally-declared " +
            "VOs), never by importing another aggregate's namespace. " +
            "Cross-aggregate rules belong in DomainPolicy classes, not inside " +
            "aggregates.\nOffenders:\n  - " + string.Join("\n  - ", offenders));
    }

    // ─────────────────────────────────────────────────────────────────────
    // Phase 3 — Event semantic guard: <Entity><Action>Event, canonical IDs
    // only. No Guid/string *Id parameters.
    // ─────────────────────────────────────────────────────────────────────

    private static readonly Regex EventRecordSignature = new(
        @"public\s+sealed\s+record\s+(?<name>\w+)\s*\((?<params>[^)]*)\)",
        RegexOptions.Compiled);

    private static readonly Regex PrimitiveIdParam = new(
        @"\b(?:Guid|string)\s+\w+Id\b",
        RegexOptions.Compiled);

    [Fact]
    public void Every_business_system_event_follows_canonical_naming_and_payload_rules()
    {
        Assert.True(Directory.Exists(BusinessSystemRoot),
            $"Business-system root missing at {BusinessSystemRoot}.");

        var namingOffenders = new List<string>();
        var payloadOffenders = new List<string>();

        foreach (var path in EnumerateEventFiles())
        {
            var filename = Path.GetFileNameWithoutExtension(path);
            if (!filename.EndsWith("Event", StringComparison.Ordinal))
            {
                namingOffenders.Add(
                    $"{Path.GetRelativePath(RepoRoot, path)} — filename must " +
                    "end with 'Event'.");
                continue;
            }

            var text = File.ReadAllText(path);
            var signature = EventRecordSignature.Match(text);

            if (!signature.Success)
            {
                namingOffenders.Add(
                    $"{Path.GetRelativePath(RepoRoot, path)} — no " +
                    "`public sealed record <Name>Event(...)` declaration found.");
                continue;
            }

            var recordName = signature.Groups["name"].Value;
            if (recordName != filename)
            {
                namingOffenders.Add(
                    $"{Path.GetRelativePath(RepoRoot, path)} — record name " +
                    $"'{recordName}' does not match filename '{filename}'.");
            }

            var parameters = signature.Groups["params"].Value;
            if (PrimitiveIdParam.IsMatch(parameters))
            {
                payloadOffenders.Add(
                    $"{Path.GetRelativePath(RepoRoot, path)} — record " +
                    $"parameters contain a `Guid *Id` or `string *Id` " +
                    $"primitive: `({parameters.Trim()})`.");
            }
        }

        Assert.True(namingOffenders.Count == 0,
            "BS-EVENT-NAMING-01 violation: business-system events must match " +
            "`<Entity><Action>Event.cs` with a matching `public sealed record " +
            "<Entity><Action>Event(...)` declaration inside.\nOffenders:\n  - " +
            string.Join("\n  - ", namingOffenders));

        Assert.True(payloadOffenders.Count == 0,
            "BS-EVENT-PAYLOAD-01 violation: business-system event payloads " +
            "must reference canonical typed IDs only. `Guid *Id` and " +
            "`string *Id` primitive relationship fields are forbidden.\n" +
            "Offenders:\n  - " + string.Join("\n  - ", payloadOffenders));
    }

    // ─────────────────────────────────────────────────────────────────────
    // Phase 4 — DomainPolicy purity: no repository/persistence references,
    // no void methods (policies return decisions).
    // ─────────────────────────────────────────────────────────────────────

    private static readonly Regex PolicyClassDeclaration = new(
        @"public\s+sealed\s+class\s+(?<name>\w+Policy)\b",
        RegexOptions.Compiled);

    private static readonly Regex PolicyVoidMethod = new(
        @"public\s+void\s+\w+\s*\(",
        RegexOptions.Compiled);

    private static readonly Regex ForbiddenPolicyTokens = new(
        @"\b(?:Repository|DbContext|IEventStore|IProducer|IConsumer|\.SaveAsync|\.SaveChanges)\b",
        RegexOptions.Compiled);

    [Fact]
    public void Business_system_domain_policies_are_pure_decision_functions()
    {
        Assert.True(Directory.Exists(BusinessSystemRoot),
            $"Business-system root missing at {BusinessSystemRoot}.");

        var voidOffenders = new List<string>();
        var tokenOffenders = new List<string>();

        foreach (var path in EnumeratePolicyFiles())
        {
            var text = StripCommentsAndStrings(File.ReadAllText(path));

            if (!PolicyClassDeclaration.IsMatch(text))
                continue; // decision / result record types are allowed here too

            if (PolicyVoidMethod.IsMatch(text))
            {
                voidOffenders.Add(Path.GetRelativePath(RepoRoot, path));
            }

            foreach (Match match in ForbiddenPolicyTokens.Matches(text))
            {
                tokenOffenders.Add(
                    $"{Path.GetRelativePath(RepoRoot, path)} — forbidden token " +
                    $"`{match.Value}`.");
            }
        }

        Assert.True(voidOffenders.Count == 0,
            "BS-POLICY-DECISION-01 violation: `*Policy` classes under " +
            "`src/domain/business-system/**/policy/` must return a decision " +
            "from every public method. `void` methods imply a side effect and " +
            "are forbidden.\nOffenders:\n  - " + string.Join("\n  - ", voidOffenders));

        Assert.True(tokenOffenders.Count == 0,
            "BS-POLICY-PURITY-01 violation: `*Policy` classes must not " +
            "reference repositories, persistence contexts, event stores, or " +
            "Kafka primitives. Policies receive aggregate state as value " +
            "inputs and return pure decisions.\nOffenders:\n  - " +
            string.Join("\n  - ", tokenOffenders));
    }

    // ─────────────────────────────────────────────────────────────────────
    // Phase 5 — Primitive-ID lock across all of business-system (not just
    // aggregates). Entities, VOs, events, policies — no file may declare a
    // Guid/string *Id property or method parameter.
    // ─────────────────────────────────────────────────────────────────────

    private static readonly Regex PrimitiveIdProperty = new(
        @"public\s+(?:Guid|string)\s+\w+Id\b",
        RegexOptions.Compiled);

    private static readonly Regex PrimitiveIdParameter = new(
        @"\(\s*(?:Guid|string)\s+\w+Id\b|,\s*(?:Guid|string)\s+\w+Id\b",
        RegexOptions.Compiled);

    [Fact]
    public void Zero_primitive_ID_declarations_remain_in_business_system()
    {
        Assert.True(Directory.Exists(BusinessSystemRoot),
            $"Business-system root missing at {BusinessSystemRoot}.");

        var propertyOffenders = new List<string>();
        var parameterOffenders = new List<string>();

        foreach (var path in Directory.EnumerateFiles(BusinessSystemRoot, "*.cs",
                     SearchOption.AllDirectories))
        {
            var text = StripCommentsAndStrings(File.ReadAllText(path));

            foreach (Match match in PrimitiveIdProperty.Matches(text))
            {
                propertyOffenders.Add(
                    $"{Path.GetRelativePath(RepoRoot, path)} — `{match.Value}`.");
            }

            foreach (Match match in PrimitiveIdParameter.Matches(text))
            {
                parameterOffenders.Add(
                    $"{Path.GetRelativePath(RepoRoot, path)} — `{match.Value.Trim()}`.");
            }
        }

        Assert.True(propertyOffenders.Count == 0,
            "BS-PRIM-ID-PROP-01 violation: business-system declares a " +
            "primitive-typed `*Id` property. Every identifier must be a typed " +
            "value object.\nOffenders:\n  - " +
            string.Join("\n  - ", propertyOffenders));

        Assert.True(parameterOffenders.Count == 0,
            "BS-PRIM-ID-PARAM-01 violation: business-system declares a " +
            "primitive-typed `*Id` method/constructor parameter. Every " +
            "identifier must be a typed value object on the wire.\n" +
            "Offenders:\n  - " + string.Join("\n  - ", parameterOffenders));
    }

    // ─────────────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────────────

    private static IEnumerable<string> EnumerateAggregateFiles() =>
        Directory.EnumerateFiles(BusinessSystemRoot, "*Aggregate.cs",
            SearchOption.AllDirectories);

    private static IEnumerable<string> EnumerateEventFiles() =>
        Directory.EnumerateFiles(BusinessSystemRoot, "*.cs",
                SearchOption.AllDirectories)
            .Where(p => p.Replace('\\', '/').Contains("/event/",
                StringComparison.Ordinal));

    private static IEnumerable<string> EnumeratePolicyFiles() =>
        Directory.EnumerateFiles(BusinessSystemRoot, "*.cs",
                SearchOption.AllDirectories)
            .Where(p => p.Replace('\\', '/').Contains("/policy/",
                StringComparison.Ordinal));

    private static readonly Regex LineComment = new(
        @"//[^\n]*", RegexOptions.Compiled);
    private static readonly Regex BlockComment = new(
        @"/\*[\s\S]*?\*/", RegexOptions.Compiled);
    private static readonly Regex StringLiteral = new(
        @"""(?:\\.|[^""\\])*""", RegexOptions.Compiled);

    private static string StripCommentsAndStrings(string text)
    {
        text = BlockComment.Replace(text, " ");
        text = LineComment.Replace(text, " ");
        text = StringLiteral.Replace(text, "\"\"");
        return text;
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null &&
               !File.Exists(Path.Combine(dir.FullName, "Whycespace.sln")) &&
               !Directory.Exists(Path.Combine(dir.FullName, "src")))
        {
            dir = dir.Parent;
        }
        return dir?.FullName ?? AppContext.BaseDirectory;
    }
}
