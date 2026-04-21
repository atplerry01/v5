using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Whycespace.Domain.BusinessSystem.Order.OrderCore.Order;
using Whycespace.Domain.ContentSystem.Document.CoreObject.Document;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Tests.Unit.Domain;

/// <summary>
/// Regression lock for the Content-System Extraction Pass (prompt
/// <c>20260421-010348-content-system-extraction</c>). Pins:
///
/// <list type="bullet">
///   <item><b>Phase 5 — Structural Binding</b>: `DocumentAggregate.Create`
///   rejects an empty <c>StructuralOwnerRef</c>. Content cannot exist without
///   a structural parent.</item>
///   <item><b>Phase 3 — Lifecycle</b>: `DocumentStatus` enum carries all four
///   canonical states (Draft / Active / Archived / Superseded).</item>
///   <item><b>Phase 7 — Event model</b>: the three new canonical events
///   (<c>DocumentVersionAttachedEvent</c>, <c>DocumentSupersededEvent</c>,
///   <c>DocumentCreatedEvent</c> with <c>StructuralOwnerRef</c>) exist.</item>
///   <item><b>Phase 6 — Business-aggregate decoupling</b>: the canonical
///   proof path <c>OrderAggregate</c> no longer declares a
///   <c>string OrderDescription</c> / <c>string Description</c> prose field;
///   it exposes a <c>ContentRef</c> instead.</item>
///   <item><b>Phase 9 — Structural regression lock</b>: no file under
///   <c>src/domain/content-system/document/core-object/document/aggregate/</c>
///   may reintroduce a prose content field on the document aggregate root.</item>
/// </list>
///
/// Scans are text-based where appropriate, type/behavior-based where runtime
/// semantics matter. Matches the existing <c>StructuralEnforcementArchTests</c>
/// and <c>BusinessSystemEnforcementLockTests</c> style.
/// </summary>
public sealed class ContentSystemEnforcementLockTests
{
    private static readonly string RepoRoot = FindRepoRoot();
    private static readonly string DomainRoot = Path.Combine(RepoRoot, "src", "domain");

    // ─────────────────────────────────────────────────────────────────────
    // Phase 5 — Structural binding is mandatory at construction
    // ─────────────────────────────────────────────────────────────────────

    [Fact]
    public void DocumentAggregate_Create_rejects_empty_StructuralOwnerRef()
    {
        var documentId = new DocumentId(Guid.NewGuid());
        var title = new DocumentTitle("Sample");
        var type = DocumentType.Generic;
        var classification = DocumentClassification.Public;
        var emptyStructural = default(StructuralOwnerRef);
        var noBusiness = BusinessOwnerRef.None;
        var createdAt = new Timestamp(DateTimeOffset.Parse("2026-04-21T00:00:00Z"));

        var ex = Assert.Throws<DomainException>(() =>
            DocumentAggregate.Create(
                documentId,
                title,
                type,
                classification,
                emptyStructural,
                noBusiness,
                createdAt));

        Assert.Contains("structural owner", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void DocumentAggregate_Create_accepts_non_empty_StructuralOwnerRef()
    {
        var documentId = new DocumentId(Guid.NewGuid());
        var title = new DocumentTitle("Sample");
        var type = DocumentType.Generic;
        var classification = DocumentClassification.Public;
        var structural = new StructuralOwnerRef(Guid.NewGuid());
        var noBusiness = BusinessOwnerRef.None;
        var createdAt = new Timestamp(DateTimeOffset.Parse("2026-04-21T00:00:00Z"));

        var doc = DocumentAggregate.Create(
            documentId, title, type, classification, structural, noBusiness, createdAt);

        Assert.Equal(DocumentStatus.Draft, doc.Status);
        Assert.Equal(structural, doc.StructuralOwner);
        Assert.False(doc.BusinessOwner.IsSet);
    }

    // ─────────────────────────────────────────────────────────────────────
    // Phase 3 — Lifecycle enum has all four states
    // ─────────────────────────────────────────────────────────────────────

    [Fact]
    public void DocumentStatus_enum_declares_the_four_canonical_lifecycle_states()
    {
        var declared = Enum.GetNames<DocumentStatus>().ToHashSet();
        var expected = new[] { "Draft", "Active", "Archived", "Superseded" };

        foreach (var state in expected)
        {
            Assert.True(declared.Contains(state),
                $"DocumentStatus is missing the canonical lifecycle state '{state}'.");
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // Phase 7 — New events are declared in the document context
    // ─────────────────────────────────────────────────────────────────────

    [Fact]
    public void Canonical_content_events_are_declared()
    {
        var types = typeof(DocumentAggregate).Assembly.GetTypes()
            .Where(t => t.Namespace == typeof(DocumentAggregate).Namespace)
            .Select(t => t.Name)
            .ToHashSet();

        foreach (var expected in new[]
                 {
                     nameof(DocumentCreatedEvent),
                     nameof(DocumentVersionAttachedEvent),
                     nameof(DocumentSupersededEvent),
                     nameof(DocumentActivatedEvent),
                     nameof(DocumentArchivedEvent),
                 })
        {
            Assert.True(types.Contains(expected),
                $"Content event '{expected}' is missing from the document context.");
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // Phase 6 / CORRECTION — OrderAggregate keeps a TYPED VO, NOT a DocumentRef.
    // OrderDescription is a structural descriptor (set at creation, not
    // versioned, not evolving) — externalising it would be scope creep.
    // ─────────────────────────────────────────────────────────────────────

    [Fact]
    public void OrderAggregate_uses_typed_OrderDescription_VO_not_externalised_DocumentRef()
    {
        var orderAggregatePath = Path.Combine(
            DomainRoot, "business-system", "order", "order-core", "order",
            "aggregate", "OrderAggregate.cs");

        Assert.True(File.Exists(orderAggregatePath),
            $"OrderAggregate.cs missing at {orderAggregatePath}.");

        var text = StripCommentsAndStrings(File.ReadAllText(orderAggregatePath));

        // MUST NOT have a raw-string prose field (would be embedded content).
        var prosePattern = new Regex(
            @"public\s+string\s+(?:OrderDescription|Description|Body|Notes|Comment)\b",
            RegexOptions.Compiled);

        var proseHits = prosePattern.Matches(text).Select(m => m.Value).ToList();

        Assert.True(proseHits.Count == 0,
            "CS-ORDER-NO-PROSE-01 violation: `OrderAggregate` declares an " +
            "embedded prose string field. Use the typed `OrderDescription` " +
            "value object.\nOffending:\n  - " + string.Join("\n  - ", proseHits));

        // MUST NOT have been externalised to a DocumentRef — OrderDescription
        // is non-evolving structural metadata, not content.
        var externalisedPattern = new Regex(
            @"public\s+DocumentRef\s+Description\b",
            RegexOptions.Compiled);

        Assert.False(externalisedPattern.IsMatch(text),
            "CS-ORDER-NOT-EXTERNALISED-01 violation: `OrderAggregate` MUST use " +
            "the typed `OrderDescription` VO, NOT a `DocumentRef`. OrderDescription " +
            "is a non-evolving structural descriptor — content-system externalisation " +
            "is reserved for true evolving content.");

        Assert.Contains("OrderDescription Description", File.ReadAllText(orderAggregatePath));
    }

    // ─────────────────────────────────────────────────────────────────────
    // Phase 6 / Externalisation sites — the three true-content aggregates
    // must all carry `DocumentRef` instead of prose.
    // ─────────────────────────────────────────────────────────────────────

    [Fact]
    public void EnforcementRuleAggregate_Description_is_a_DocumentRef()
    {
        var path = Path.Combine(
            DomainRoot, "economic-system", "enforcement", "rule",
            "aggregate", "EnforcementRuleAggregate.cs");

        Assert.True(File.Exists(path), $"EnforcementRuleAggregate.cs missing at {path}.");

        var text = StripCommentsAndStrings(File.ReadAllText(path));

        Assert.False(
            new Regex(@"public\s+string\s+Description\b").IsMatch(text),
            "CS-DECOUPLE-RULE-01 violation: `EnforcementRuleAggregate.Description` " +
            "must be a `DocumentRef`, not a raw string.");

        Assert.False(
            new Regex(@"public\s+RuleDescription\s+Description\b").IsMatch(text),
            "CS-DECOUPLE-RULE-01 violation: `EnforcementRuleAggregate.Description` " +
            "still wraps prose via `RuleDescription` — externalise to `DocumentRef`.");

        Assert.Matches(
            new Regex(@"public\s+DocumentRef\s+Description\b"),
            text);
    }

    [Fact]
    public void AuditRecordAggregate_EvidenceSummary_is_a_DocumentRef()
    {
        var path = Path.Combine(
            DomainRoot, "economic-system", "compliance", "audit",
            "aggregate", "AuditRecordAggregate.cs");

        Assert.True(File.Exists(path), $"AuditRecordAggregate.cs missing at {path}.");

        var text = StripCommentsAndStrings(File.ReadAllText(path));

        Assert.False(
            new Regex(@"public\s+EvidenceSummary\s+EvidenceSummary\b").IsMatch(text),
            "CS-DECOUPLE-EVIDENCE-01 violation: `AuditRecordAggregate.EvidenceSummary` " +
            "still wraps prose via `EvidenceSummary` VO — externalise to `DocumentRef`.");

        Assert.Matches(
            new Regex(@"public\s+DocumentRef\s+EvidenceSummary\b"),
            text);
    }

    [Fact]
    public void KanbanCard_Title_is_typed_VO_and_Description_is_DocumentRef()
    {
        var path = Path.Combine(
            DomainRoot, "operational-system", "sandbox", "kanban",
            "entity", "KanbanCard.cs");

        Assert.True(File.Exists(path), $"KanbanCard.cs missing at {path}.");

        var text = StripCommentsAndStrings(File.ReadAllText(path));

        // Title must be a typed VO (not raw string, not DocumentRef).
        Assert.False(
            new Regex(@"public\s+string\s+Title\b").IsMatch(text),
            "CS-KANBAN-TITLE-TYPED-01 violation: `KanbanCard.Title` must be a " +
            "typed `KanbanCardTitle` VO, not a raw `string`.");
        Assert.False(
            new Regex(@"public\s+DocumentRef\s+Title\b").IsMatch(text),
            "CS-KANBAN-TITLE-NOT-EXTERNALISED-01 violation: `KanbanCard.Title` " +
            "is structural identity — must NOT be externalised to `DocumentRef`.");
        Assert.Matches(new Regex(@"public\s+KanbanCardTitle\s+Title\b"), text);

        // Description must be a DocumentRef (externalised content).
        Assert.False(
            new Regex(@"public\s+string\s+Description\b").IsMatch(text),
            "CS-DECOUPLE-KANBAN-01 violation: `KanbanCard.Description` must be " +
            "a `DocumentRef`, not a raw `string`.");
        Assert.Matches(new Regex(@"public\s+DocumentRef\s+Description\b"), text);
    }

    // ─────────────────────────────────────────────────────────────────────
    // ContentId indirection — every local DocumentRef wraps the canonical
    // shared-kernel ContentId, never a raw Guid.
    // ─────────────────────────────────────────────────────────────────────

    [Fact]
    public void Every_local_DocumentRef_wraps_ContentId_not_raw_Guid()
    {
        var refPaths = new[]
        {
            Path.Combine(DomainRoot, "economic-system", "enforcement", "rule", "value-object", "DocumentRef.cs"),
            Path.Combine(DomainRoot, "economic-system", "compliance", "audit", "value-object", "DocumentRef.cs"),
            Path.Combine(DomainRoot, "operational-system", "sandbox", "kanban", "value-object", "DocumentRef.cs"),
        };

        var offenders = new List<string>();

        foreach (var path in refPaths)
        {
            Assert.True(File.Exists(path), $"Expected DocumentRef at {path}.");
            var text = StripCommentsAndStrings(File.ReadAllText(path));

            if (new Regex(@"public\s+Guid\s+Value\b").IsMatch(text))
            {
                offenders.Add(
                    $"{Path.GetRelativePath(RepoRoot, path)} — `Value` is raw `Guid`. " +
                    "Must wrap `ContentId`.");
                continue;
            }

            if (!new Regex(@"public\s+ContentId\s+Value\b").IsMatch(text))
            {
                offenders.Add(
                    $"{Path.GetRelativePath(RepoRoot, path)} — `Value` is not a " +
                    "`ContentId`. Every DocumentRef MUST wrap the canonical " +
                    "`Whycespace.Domain.SharedKernel.Primitive.Identity.ContentId`.");
            }
        }

        Assert.True(offenders.Count == 0,
            "CS-DOCREF-WRAPS-CONTENTID-01 violation: every local `DocumentRef` " +
            "VO MUST wrap the canonical shared-kernel `ContentId`, not a raw Guid.\n" +
            "Offenders:\n  - " + string.Join("\n  - ", offenders));
    }

    // ─────────────────────────────────────────────────────────────────────
    // DocumentRef misuse guard — DocumentRef MUST NOT be used for name /
    // label / code / identifier fields. Those are structural, not content.
    // ─────────────────────────────────────────────────────────────────────

    [Fact]
    public void DocumentRef_is_not_used_for_name_label_code_or_identifier_fields()
    {
        // Forbidden field-name suffixes: Name, Label, Code, Identifier.
        // Also Id-suffix: DocumentRef wraps ContentId already; using it as an
        // *Id field would double-wrap and confuse semantics.
        var misusePattern = new Regex(
            @"public\s+DocumentRef\s+\w*(?:Name|Label|Code|Identifier|Id)\b(?!\s*[<(])",
            RegexOptions.Compiled);

        var offenders = new List<string>();

        foreach (var path in Directory.EnumerateFiles(DomainRoot, "*.cs", SearchOption.AllDirectories))
        {
            // Skip the DocumentRef definitions themselves — they legitimately
            // declare a single canonical shape.
            if (path.Replace('\\', '/').EndsWith("/DocumentRef.cs", StringComparison.OrdinalIgnoreCase))
                continue;

            var text = StripCommentsAndStrings(File.ReadAllText(path));

            foreach (Match m in misusePattern.Matches(text))
            {
                offenders.Add(
                    $"{Path.GetRelativePath(RepoRoot, path)} — `{m.Value}`.");
            }
        }

        Assert.True(offenders.Count == 0,
            "CS-DOCREF-MISUSE-01 violation: `DocumentRef` MUST NOT back a " +
            "name / label / code / identifier field. Those are structural " +
            "identity — use a typed VO instead (e.g. `*Name`, `*Code`, `*Id`). " +
            "`DocumentRef` is reserved for true evolving content.\nOffenders:\n  - " +
            string.Join("\n  - ", offenders));
    }

    // ─────────────────────────────────────────────────────────────────────
    // Inverse lock: aggregates flagged as structural-only (label / identity)
    // MUST NOT be externalised to DocumentRef. This prevents overzealous
    // future externalisation of non-evolving labels.
    // ─────────────────────────────────────────────────────────────────────

    [Fact]
    public void Structural_labels_remain_typed_VOs_and_do_not_externalise_to_DocumentRef()
    {
        var checks = new (string relativePath, string fieldName)[]
        {
            (Path.Combine("economic-system", "enforcement", "violation", "aggregate", "ViolationAggregate.cs"), "Reason"),
            (Path.Combine("economic-system", "transaction", "settlement", "aggregate", "SettlementAggregate.cs"), "FailureReason"),
            (Path.Combine("business-system", "agreement", "party-governance", "counterparty", "entity", "CounterpartyProfile.cs"), "Name"),
            (Path.Combine("operational-system", "sandbox", "kanban", "aggregate", "KanbanAggregate.cs"), "Name"),
        };

        var offenders = new List<string>();

        foreach (var (relativePath, fieldName) in checks)
        {
            var fullPath = Path.Combine(DomainRoot, relativePath);
            if (!File.Exists(fullPath))
                continue; // pre-migration file may not exist in all branches

            var text = StripCommentsAndStrings(File.ReadAllText(fullPath));
            var externalisedPattern = new Regex($@"public\s+DocumentRef\s+{fieldName}\b");

            if (externalisedPattern.IsMatch(text))
                offenders.Add($"{relativePath}: {fieldName} externalised to DocumentRef");
        }

        Assert.True(offenders.Count == 0,
            "CS-NO-OVER-EXTERNALISE-01 violation: structural labels / audit reasons / identity " +
            "fields MUST NOT be externalised to `DocumentRef`. Content-system extraction is " +
            "reserved for true evolving content.\nOffenders:\n  - " +
            string.Join("\n  - ", offenders));
    }

    // ─────────────────────────────────────────────────────────────────────
    // Phase 9 — Document aggregate root may not reintroduce embedded prose
    // ─────────────────────────────────────────────────────────────────────

    [Fact]
    public void DocumentAggregate_declares_no_embedded_prose_content_fields()
    {
        var docAggregatePath = Path.Combine(
            DomainRoot, "content-system", "document", "core-object", "document",
            "aggregate", "DocumentAggregate.cs");

        Assert.True(File.Exists(docAggregatePath),
            $"DocumentAggregate.cs missing at {docAggregatePath}.");

        var text = StripCommentsAndStrings(File.ReadAllText(docAggregatePath));

        // DocumentAggregate is a METADATA aggregate: it owns identity, title
        // VO, type VO, classification VO, status, and refs. It must NEVER
        // hold raw prose — body bytes live on the File/Version artifact.
        var prosePattern = new Regex(
            @"public\s+string\s+(?:Body|Content|Payload|Description|Notes|Comment|Html|Markdown|Abstract|Blurb)\b",
            RegexOptions.Compiled);

        var hits = prosePattern.Matches(text).Select(m => m.Value).ToList();

        Assert.True(hits.Count == 0,
            "CS-DOC-PROSE-LOCK-01 violation: `DocumentAggregate` declares a " +
            "raw-string prose property. The content aggregate owns identity " +
            "and metadata; content bytes belong to the version/file artifacts.\n" +
            "Offending declarations:\n  - " + string.Join("\n  - ", hits));
    }

    // ─────────────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────────────

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
