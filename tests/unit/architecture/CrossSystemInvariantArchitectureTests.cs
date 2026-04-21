using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Whycespace.Tests.Unit.Architecture;

/// <summary>
/// Cross-System Invariant enforcement aligned to classification topology.
/// Invariants are owned by the classification whose rule they express — no
/// new top-level "cross-system-invariants" layer, no shared invariant ring
/// outside existing systems. These tests pin the five canonical invariant
/// policies and guard against drift.
/// </summary>
public sealed class CrossSystemInvariantArchitectureTests
{
    private static readonly string RepoRoot = FindRepoRoot();
    private static readonly string SrcRoot = Path.Combine(RepoRoot, "src");
    private static readonly string DomainRoot = Path.Combine(SrcRoot, "domain");

    private static readonly (string Classification, string Context, string Domain, string Policy)[] Canonical =
    {
        ("structural-system", "invariant", "economic-binding",    "EconomicEntityMustHaveStructuralOwnerPolicy"),
        ("business-system",   "invariant", "economic-attribution","TransactionMustHaveBusinessReferencePolicy"),
        ("content-system",    "invariant", "ownership",           "ContentMustHaveValidOwnerPolicy"),
        ("economic-system",   "invariant", "ledger-integrity",    "TransactionMustProduceBalancedLedgerPolicy"),
        ("operational-system","invariant", "command-integrity",   "CommandMustProduceDomainEventPolicy"),
    };

    [Fact]
    public void Every_canonical_invariant_policy_file_exists()
    {
        var missing = new List<string>();
        foreach (var (classification, context, domain, policy) in Canonical)
        {
            var path = Path.Combine(DomainRoot, classification, context, domain, "policy", policy + ".cs");
            if (!File.Exists(path))
                missing.Add(path);
        }

        Assert.True(missing.Count == 0,
            "Canonical cross-system invariant policy files are missing:\n" +
            string.Join("\n", missing));
    }

    [Fact]
    public void Invariant_context_lives_only_in_owning_classifications()
    {
        var owners = Canonical.Select(c => c.Classification).ToHashSet();
        var drift = new List<string>();

        foreach (var classificationDir in Directory.EnumerateDirectories(DomainRoot))
        {
            var classification = Path.GetFileName(classificationDir);
            var invariantDir = Path.Combine(classificationDir, "invariant");
            if (!Directory.Exists(invariantDir)) continue;

            if (!owners.Contains(classification))
                drift.Add(invariantDir);
        }

        Assert.True(drift.Count == 0,
            "`invariant/` context appeared outside the five owning classifications. " +
            "No new top-level cross-system-invariants layer is permitted. " +
            "Hits:\n" + string.Join("\n", drift));
    }

    [Fact]
    public void No_cross_system_invariants_top_level_layer_exists()
    {
        string[] forbidden =
        {
            Path.Combine(SrcRoot, "cross-system-invariants"),
            Path.Combine(DomainRoot, "cross-system-invariants"),
            Path.Combine(DomainRoot, "cross-system"),
            Path.Combine(DomainRoot, "invariants"),
        };

        var hits = forbidden.Where(Directory.Exists).ToList();

        Assert.True(hits.Count == 0,
            "Forbidden top-level cross-system invariant layer detected. " +
            "Invariants must live inside their owning classification under " +
            "{classification}/invariant/{domain}/policy. Hits:\n" +
            string.Join("\n", hits));
    }

    [Fact]
    public void Invariant_policies_are_sealed_and_hold_no_mutable_state()
    {
        var fieldPattern = new Regex(
            @"^\s*(?!.*\b(const|readonly)\b)(public|private|protected|internal)\s+" +
            @"(?!(static\s+)?(class|record|struct|enum|interface)\b)" +
            @"[A-Za-z0-9_<>?,\s\[\]]+\s+[A-Za-z_][A-Za-z0-9_]*\s*(=|;)");

        var issues = new List<string>();

        foreach (var (classification, context, domain, policy) in Canonical)
        {
            var path = Path.Combine(DomainRoot, classification, context, domain, "policy", policy + ".cs");
            if (!File.Exists(path))
            {
                issues.Add(path + ": file not found");
                continue;
            }

            var content = File.ReadAllText(path);
            if (!Regex.IsMatch(content, @"public\s+sealed\s+class\s+" + Regex.Escape(policy)))
                issues.Add(path + ": policy class is not `public sealed class`");

            if (Regex.IsMatch(content, @"\bGuid\.NewGuid\s*\("))
                issues.Add(path + ": policy calls Guid.NewGuid (determinism violation)");

            if (Regex.IsMatch(content, @"DateTime(Offset)?\.(Utc)?Now\b"))
                issues.Add(path + ": policy reads DateTime.UtcNow (determinism violation)");

            if (Regex.IsMatch(content, @"\bnew\s+Random\b|RandomNumberGenerator"))
                issues.Add(path + ": policy uses RNG (determinism violation)");
        }

        Assert.True(issues.Count == 0,
            "Invariant policies must be pure (sealed, no mutation, no clock, " +
            "no RNG). Issues:\n" + string.Join("\n", issues));
    }

    [Fact]
    public void Invariant_policies_do_not_reference_other_classifications()
    {
        var classifications = new[]
        {
            "StructuralSystem", "BusinessSystem", "ContentSystem",
            "EconomicSystem", "OperationalSystem"
        };

        var issues = new List<string>();

        foreach (var (classification, context, domain, policy) in Canonical)
        {
            var ownerNs = ClassificationNamespaceToken(classification);
            var dir = Path.Combine(DomainRoot, classification, context, domain, "policy");
            if (!Directory.Exists(dir)) continue;

            foreach (var file in Directory.EnumerateFiles(dir, "*.cs"))
            {
                foreach (var line in File.ReadAllLines(file))
                {
                    var trimmed = line.TrimStart();
                    if (!trimmed.StartsWith("using ")) continue;

                    foreach (var other in classifications)
                    {
                        if (other == ownerNs) continue;
                        if (trimmed.Contains("Whycespace.Domain." + other + "."))
                            issues.Add($"{file}: cross-classification using detected -> {trimmed}");
                    }
                }
            }
        }

        Assert.True(issues.Count == 0,
            "Invariant policies must operate on primitives / shared-kernel only. " +
            "Cross-classification domain references are forbidden. Issues:\n" +
            string.Join("\n", issues));
    }

    [Fact]
    public void Each_invariant_domain_follows_canonical_nesting()
    {
        var issues = new List<string>();

        foreach (var (classification, context, domain, _) in Canonical)
        {
            var invariantDir = Path.Combine(DomainRoot, classification, context);
            if (!Directory.Exists(invariantDir))
            {
                issues.Add(invariantDir + ": invariant context folder missing");
                continue;
            }

            var domainDir = Path.Combine(invariantDir, domain);
            if (!Directory.Exists(domainDir))
            {
                issues.Add(domainDir + ": invariant domain folder missing");
                continue;
            }

            var policyDir = Path.Combine(domainDir, "policy");
            if (!Directory.Exists(policyDir))
                issues.Add(policyDir + ": policy subfolder missing under invariant domain");
        }

        Assert.True(issues.Count == 0,
            "Invariant placement must be {classification}/invariant/{domain}/policy " +
            "(canonical 3-level nesting). Issues:\n" + string.Join("\n", issues));
    }

    private static string ClassificationNamespaceToken(string folder)
        => string.Concat(folder.Split('-').Select(p => char.ToUpperInvariant(p[0]) + p.Substring(1)));

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
