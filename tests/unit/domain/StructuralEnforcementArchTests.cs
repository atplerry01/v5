using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Whycespace.Tests.Unit.Domain;

/// <summary>
/// Structural enforcement arch tests — pins the two rules promoted into
/// <c>domain.guard.md</c> on 2026-04-20 under the D-ID-REF-01 migration pass:
///
/// <list type="bullet">
///   <item><b>D-ID-REF-01 (S1)</b> — no aggregate root exposes a raw
///   <c>Guid *Id</c> inter-aggregate reference property. Every FK on an
///   aggregate root is a typed value-object (primary <c>*Id</c> VO or
///   canonical <c>*Ref</c> VO).</item>
///   <item><b>D-INV-NON-EMPTY-01 (S2)</b> — every <c>AggregateRoot</c>
///   subclass has a non-empty <c>EnsureInvariants()</c> body; a comment-only
///   body silently allows invariant drift and is treated as absent.</item>
/// </list>
///
/// <para>The test is regex-based, matching the R4.A observability arch-test
/// style (text-based scans, no Roslyn dependency). The scan targets
/// <c>src/domain/**/*Aggregate.cs</c> — the only layer where aggregate-root
/// inter-relationship references are declared.</para>
/// </summary>
public sealed class StructuralEnforcementArchTests
{
    private static readonly string RepoRoot = FindRepoRoot();
    private static readonly string DomainRoot = Path.Combine(RepoRoot, "src", "domain");

    // D-ID-REF-01: `public Guid XxxId { get; private set; }` on an aggregate
    // root is forbidden. Transient `Guid` parameters on methods (command
    // inputs) are not covered by this pattern — only property declarations.
    private static readonly Regex RawGuidIdProperty = new(
        @"public\s+Guid\s+\w+Id\s*\{\s*get",
        RegexOptions.Compiled);

    // D-INV-NON-EMPTY-01: catches `EnsureInvariants()` overrides whose body
    // is empty, whitespace, or nothing but a single-line `//` comment.
    private static readonly Regex EmptyEnsureInvariants = new(
        @"protected\s+override\s+void\s+EnsureInvariants\s*\(\s*\)\s*\{\s*(//[^\n]*\s*)?\}",
        RegexOptions.Compiled);

    // D-CONTENT-STR-EMBED-01: catches raw descriptive content-like string
    // properties on aggregate roots. Matches literal property names the user
    // spec calls out: Description, Body, Content, Notes. Typed descriptive
    // VOs (RuleDescription, RuleName, ExpenseLineDescription) and content
    // references (DocumentRef, ContentRef) pass this scan.
    private static readonly Regex RawContentLikeStringProperty = new(
        @"public\s+string\s+(Description|Body|Content|Notes)\s*\{",
        RegexOptions.Compiled);

    // D-VO-TYPING-ECON-01: catches raw `decimal` properties on aggregate
    // roots under economic-system/**. Value-bearing fields (amount, balance,
    // price, rate) must be wrapped in typed VOs (Amount, ExchangeRate, etc.).
    private static readonly Regex RawDecimalProperty = new(
        @"public\s+decimal\s+\w+\s*\{\s*get",
        RegexOptions.Compiled);

    // D-TXN-IMMUTABLE-01: catches public setters on aggregate roots under
    // economic-system/transaction/**. Transactions mutate only via event
    // application; every property is `{ get; private set; }` or equivalent.
    private static readonly Regex PublicSetter = new(
        @"\{\s*get;\s*set;\s*\}",
        RegexOptions.Compiled);

    // D-BALANCE-APPLY-ONLY-01: checks that every economic-system aggregate
    // root declares an `Apply(object domainEvent)` override. Combined with
    // the event-sourced base class pattern (state mutation only inside the
    // Apply switch), this pins "balance mutated only via Apply()." The
    // parameter name may carry the C# `@` keyword-escape prefix (e.g.,
    // `object @event`), so the regex permits an optional leading `@`.
    private static readonly Regex ApplyOverride = new(
        @"protected\s+override\s+void\s+Apply\s*\(\s*object\s+@?\w+\s*\)",
        RegexOptions.Compiled);

    // D-AGG-NO-CROSS-01: catches calls of the form `OtherAggregate.Method(...)`
    // from inside an aggregate file. Self-references (calls to the aggregate's
    // own static factory, e.g. from a compensation overload) are excluded by
    // comparing the captured type name to the file's own public class name.
    private static readonly Regex AggregateMethodCall = new(
        @"(\w+Aggregate)\.\w+\s*\(",
        RegexOptions.Compiled);

    private static readonly Regex PublicAggregateClassName = new(
        @"public\s+(?:sealed\s+)?(?:abstract\s+)?class\s+(\w+Aggregate)\b",
        RegexOptions.Compiled);

    // D-DOM-NO-INFRA-01: no infrastructure abstractions may be referenced
    // from `src/domain/**`. The domain layer is a pure DDD core and must
    // know nothing about persistence, event-store implementation, database
    // contexts, or dependency-injection containers.
    private static readonly Regex InfraAbstraction = new(
        @"\b(IRepository|IEventStore|DbContext|IServiceProvider)\b",
        RegexOptions.Compiled);

    // D-DET-01 (extended 2026-04-20): determinism rule expands beyond the
    // original `Guid.NewGuid` / `DateTime.UtcNow` / `Random` surface to also
    // forbid temporal and async primitives in the domain layer. Aggregates
    // and domain services must be synchronous, time-agnostic, and free of
    // cooperative cancellation — temporal orchestration is the engines /
    // runtime layer's concern.
    private static readonly Regex TemporalOrAsyncPrimitive = new(
        @"\b(Task\.Delay|Thread\.Sleep|Timer|CancellationToken|async\s+Task|await)\b",
        RegexOptions.Compiled);

    // D-T1M-NO-DIRECT-MUTATION-01: no aggregate method invocations inside
    // T1M workflow Step classes. Steps dispatch commands via
    // ISystemIntentDispatcher; aggregate mutation happens in T2E handlers.
    // Reuses AggregateMethodCall regex with no self-exclusion.

    // D-DOM-SVC-STATELESS-01: catches mutable private instance fields on
    // domain services. Matches `private <type> _field[;|=]` lines with the
    // underscore-prefixed field naming convention, and excludes readonly /
    // static / const / event / async / new / override / virtual / abstract
    // modifiers. Fields of type ending in `Specification` or `Options` /
    // `Config` are treated as specification / immutable-configuration
    // holders per the rule's allow-list.
    private static readonly Regex MutableInstanceFieldOnService = new(
        @"^\s*private\s+(?!(readonly|static|const|event|async|new|override|virtual|abstract)\b)[\w<>?,\[\]\.]+\s+_\w+\s*[;=]",
        RegexOptions.Compiled | RegexOptions.Multiline);

    private static readonly Regex SpecOrConfigFieldType = new(
        @"\b(\w*Specification|\w*Options|\w*Config|\w*Settings)\b",
        RegexOptions.Compiled);

    [Fact]
    public void No_aggregate_root_declares_a_raw_Guid_inter_aggregate_reference_property()
    {
        Assert.True(Directory.Exists(DomainRoot), $"Domain root missing at {DomainRoot}.");

        var offenders = EnumerateAggregateFiles()
            .Select(path => (path, text: File.ReadAllText(path)))
            .Where(t => RawGuidIdProperty.IsMatch(t.text))
            .Select(t => Path.GetRelativePath(RepoRoot, t.path))
            .OrderBy(p => p)
            .ToList();

        Assert.True(
            offenders.Count == 0,
            "D-ID-REF-01 violation: the following aggregate root files declare raw `public Guid *Id { get; }` " +
            "properties instead of typed value objects. Every inter-aggregate reference must be a typed VO " +
            "(primary *Id VO or *Ref VO). Offenders:\n  - " + string.Join("\n  - ", offenders));
    }

    [Fact]
    public void No_aggregate_root_declares_a_raw_content_like_string_property()
    {
        Assert.True(Directory.Exists(DomainRoot), $"Domain root missing at {DomainRoot}.");

        var offenders = EnumerateAggregateFiles()
            // D-CONTENT-STR-EMBED-01 excludes content-system/** — that's the
            // canonical location where content aggregates legitimately carry
            // descriptive string payloads.
            .Where(path => !IsUnderContentSystem(path))
            .Select(path => (path, text: File.ReadAllText(path)))
            .Where(t => RawContentLikeStringProperty.IsMatch(t.text))
            .Select(t => Path.GetRelativePath(RepoRoot, t.path))
            .OrderBy(p => p)
            .ToList();

        Assert.True(
            offenders.Count == 0,
            "D-CONTENT-STR-EMBED-01 violation: the following aggregate root files declare raw " +
            "`public string Description|Body|Content|Notes { get; }` properties. Content-like strings " +
            "must either be (a) wrapped in a typed descriptive value object for short structural " +
            "labels, or (b) externalised via a content reference (ContentRef / DocumentRef) that points " +
            "into content-system/**. Offenders:\n  - " + string.Join("\n  - ", offenders));
    }

    [Fact]
    public void No_economic_aggregate_root_declares_a_raw_decimal_value_property()
    {
        Assert.True(Directory.Exists(DomainRoot), $"Domain root missing at {DomainRoot}.");

        var offenders = EnumerateAggregateFiles()
            .Where(IsUnderEconomicSystem)
            .Select(path => (path, text: File.ReadAllText(path)))
            .Where(t => RawDecimalProperty.IsMatch(t.text))
            .Select(t => Path.GetRelativePath(RepoRoot, t.path))
            .OrderBy(p => p)
            .ToList();

        Assert.True(
            offenders.Count == 0,
            "D-VO-TYPING-ECON-01 violation: the following economic-system aggregate root files declare " +
            "`public decimal <field> { get; }` properties. Monetary / rate values must be wrapped in typed " +
            "value objects (Amount, ExchangeRate, etc.). Event payloads may continue to carry `decimal` for " +
            "wire compatibility; the aggregate wraps at Apply(). Offenders:\n  - " +
            string.Join("\n  - ", offenders));
    }

    [Fact]
    public void No_transaction_aggregate_root_declares_a_public_setter()
    {
        Assert.True(Directory.Exists(DomainRoot), $"Domain root missing at {DomainRoot}.");

        var offenders = EnumerateAggregateFiles()
            .Where(IsUnderEconomicTransaction)
            .Select(path => (path, text: File.ReadAllText(path)))
            .Where(t => PublicSetter.IsMatch(t.text))
            .Select(t => Path.GetRelativePath(RepoRoot, t.path))
            .OrderBy(p => p)
            .ToList();

        Assert.True(
            offenders.Count == 0,
            "D-TXN-IMMUTABLE-01 violation: the following economic-system/transaction/** aggregate root " +
            "files declare properties with a public setter (`{ get; set; }`). Transaction aggregates mutate " +
            "exclusively through event application — every property must be `{ get; private set; }`. " +
            "Offenders:\n  - " + string.Join("\n  - ", offenders));
    }

    [Fact]
    public void Every_economic_aggregate_root_overrides_Apply_for_event_sourced_mutation()
    {
        Assert.True(Directory.Exists(DomainRoot), $"Domain root missing at {DomainRoot}.");

        var offenders = EnumerateAggregateFiles()
            .Where(IsUnderEconomicSystem)
            .Select(path => (path, text: File.ReadAllText(path)))
            .Where(t => !ApplyOverride.IsMatch(t.text))
            .Select(t => Path.GetRelativePath(RepoRoot, t.path))
            .OrderBy(p => p)
            .ToList();

        Assert.True(
            offenders.Count == 0,
            "D-BALANCE-APPLY-ONLY-01 violation: the following economic-system aggregate root files do " +
            "not override `protected override void Apply(object domainEvent)`. Every economic aggregate " +
            "must be event-sourced — state (including balance-shaped fields) may only mutate inside the " +
            "Apply switch on a raised/replayed domain event. Offenders:\n  - " +
            string.Join("\n  - ", offenders));
    }

    [Fact]
    public void No_aggregate_invokes_another_aggregates_method()
    {
        Assert.True(Directory.Exists(DomainRoot), $"Domain root missing at {DomainRoot}.");

        var offenders = new List<string>();

        foreach (var path in EnumerateAggregateFiles())
        {
            var text = StripComments(File.ReadAllText(path));
            var classMatch = PublicAggregateClassName.Match(text);
            var selfName = classMatch.Success ? classMatch.Groups[1].Value : null;

            foreach (Match m in AggregateMethodCall.Matches(text))
            {
                var calledTypeName = m.Groups[1].Value;
                if (calledTypeName == selfName) continue;
                offenders.Add($"{Path.GetRelativePath(RepoRoot, path)} calls {calledTypeName}.{m.Value.Split('.').Last()}");
                break; // one offender line per file is sufficient to fail the test
            }
        }

        Assert.True(
            offenders.Count == 0,
            "D-AGG-NO-CROSS-01 violation: the following aggregate files invoke methods on other aggregates. " +
            "Aggregate-to-aggregate coordination must happen in the engines layer (T1M sagas via " +
            "ISystemIntentDispatcher), never directly from inside an aggregate. Offenders:\n  - " +
            string.Join("\n  - ", offenders.OrderBy(o => o)));
    }

    [Fact]
    public void Domain_layer_does_not_reference_infrastructure_abstractions()
    {
        Assert.True(Directory.Exists(DomainRoot), $"Domain root missing at {DomainRoot}.");

        var offenders = Directory.EnumerateFiles(DomainRoot, "*.cs", SearchOption.AllDirectories)
            .Select(path => (path, text: StripComments(File.ReadAllText(path))))
            .Where(t => InfraAbstraction.IsMatch(t.text))
            .Select(t => Path.GetRelativePath(RepoRoot, t.path))
            .OrderBy(p => p)
            .ToList();

        Assert.True(
            offenders.Count == 0,
            "D-DOM-NO-INFRA-01 violation: the following src/domain/** files reference an infrastructure " +
            "abstraction (IRepository / IEventStore / DbContext / IServiceProvider). Domain must be a pure " +
            "DDD core with no knowledge of persistence or DI. Offenders:\n  - " +
            string.Join("\n  - ", offenders));
    }

    [Fact]
    public void Domain_layer_does_not_contain_temporal_or_async_primitives()
    {
        Assert.True(Directory.Exists(DomainRoot), $"Domain root missing at {DomainRoot}.");

        var offenders = Directory.EnumerateFiles(DomainRoot, "*.cs", SearchOption.AllDirectories)
            .Select(path => (path, text: StripComments(File.ReadAllText(path))))
            .Where(t => TemporalOrAsyncPrimitive.IsMatch(t.text))
            .Select(t => Path.GetRelativePath(RepoRoot, t.path))
            .OrderBy(p => p)
            .ToList();

        Assert.True(
            offenders.Count == 0,
            "D-DET-01 (extended) violation: the following src/domain/** files reference a forbidden " +
            "temporal or async primitive (Task.Delay / Thread.Sleep / Timer / CancellationToken / " +
            "async Task / await). Domain code must be synchronous and time-agnostic; temporal " +
            "orchestration belongs in the engines/runtime layer. Offenders:\n  - " +
            string.Join("\n  - ", offenders));
    }

    [Fact]
    public void No_T1M_workflow_step_invokes_aggregate_methods_directly()
    {
        var t1mRoot = Path.Combine(RepoRoot, "src", "engines", "T1M");
        if (!Directory.Exists(t1mRoot))
            return; // T1M optional; test passes trivially if absent.

        var offenders = new List<string>();

        foreach (var path in Directory.EnumerateFiles(t1mRoot, "*Step.cs", SearchOption.AllDirectories))
        {
            var text = StripComments(File.ReadAllText(path));
            foreach (Match m in AggregateMethodCall.Matches(text))
            {
                offenders.Add($"{Path.GetRelativePath(RepoRoot, path)} calls {m.Value}");
                break;
            }
        }

        Assert.True(
            offenders.Count == 0,
            "D-T1M-NO-DIRECT-MUTATION-01 violation: the following T1M workflow step files invoke " +
            "aggregate methods directly. Steps must dispatch commands via ISystemIntentDispatcher; " +
            "aggregate mutation happens exclusively in T2E handlers. Offenders:\n  - " +
            string.Join("\n  - ", offenders.OrderBy(o => o)));
    }

    [Fact]
    public void Domain_services_do_not_carry_mutable_instance_fields()
    {
        Assert.True(Directory.Exists(DomainRoot), $"Domain root missing at {DomainRoot}.");

        var offenders = new List<string>();

        foreach (var path in Directory.EnumerateFiles(DomainRoot, "*Service.cs", SearchOption.AllDirectories))
        {
            var text = StripComments(File.ReadAllText(path));
            foreach (Match m in MutableInstanceFieldOnService.Matches(text))
            {
                var line = m.Value.Trim();
                // D-DOM-SVC-STATELESS-01 allow-list: specification, options,
                // config, settings (immutable predicate / config holders).
                if (SpecOrConfigFieldType.IsMatch(line)) continue;
                offenders.Add($"{Path.GetRelativePath(RepoRoot, path)}: {line}");
                break;
            }
        }

        Assert.True(
            offenders.Count == 0,
            "D-DOM-SVC-STATELESS-01 violation: the following domain services declare mutable private " +
            "instance fields (not readonly / static / const, not a specification or config holder). " +
            "Domain services must be stateless — any required predicate or configuration must be injected " +
            "as a `private readonly` field. Offenders:\n  - " +
            string.Join("\n  - ", offenders.OrderBy(o => o)));
    }

    [Fact]
    public void EnsureInvariants_bodies_are_non_empty_on_every_aggregate_root()
    {
        Assert.True(Directory.Exists(DomainRoot), $"Domain root missing at {DomainRoot}.");

        var offenders = EnumerateAggregateFiles()
            .Select(path => (path, text: File.ReadAllText(path)))
            .Where(t => EmptyEnsureInvariants.IsMatch(t.text))
            .Select(t => Path.GetRelativePath(RepoRoot, t.path))
            .OrderBy(p => p)
            .ToList();

        Assert.True(
            offenders.Count == 0,
            "D-INV-NON-EMPTY-01 violation: the following aggregate root files override `EnsureInvariants()` " +
            "with an empty or comment-only body. Every aggregate's EnsureInvariants must at minimum assert " +
            "identity non-emptiness post-Apply. Offenders:\n  - " + string.Join("\n  - ", offenders));
    }

    private static IEnumerable<string> EnumerateAggregateFiles()
    {
        return Directory.EnumerateFiles(DomainRoot, "*Aggregate.cs", SearchOption.AllDirectories);
    }

    /// <summary>
    /// Strips C# comments (<c>//</c>, <c>///</c>, and <c>/* ... */</c>) from
    /// a source-file text so downstream regex scans do not match forbidden
    /// tokens that appear only inside XML doc comments or block comments
    /// (e.g. a <c>&lt;see cref="CancellationToken"/&gt;</c> reference that
    /// is documentation, not executable code).
    /// </summary>
    private static string StripComments(string text)
    {
        var noBlockComments = Regex.Replace(text, @"/\*[\s\S]*?\*/", "");
        var noLineComments = Regex.Replace(noBlockComments, @"//[^\n]*", "");
        return noLineComments;
    }

    private static bool IsUnderContentSystem(string path)
    {
        var normalized = path.Replace('\\', '/');
        return normalized.Contains("/content-system/", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsUnderEconomicSystem(string path)
    {
        var normalized = path.Replace('\\', '/');
        return normalized.Contains("/economic-system/", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsUnderEconomicTransaction(string path)
    {
        var normalized = path.Replace('\\', '/');
        return normalized.Contains("/economic-system/transaction/", StringComparison.OrdinalIgnoreCase);
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
