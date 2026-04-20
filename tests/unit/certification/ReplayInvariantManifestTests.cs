using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Whycespace.Tests.Unit.Certification;

/// <summary>
/// R5.C.1 / R-REPLAY-CERTIFICATION-REGISTRY-01 — verifies the replay-
/// equivalence catalog YAML exists, is in sync with the C# mirror, every
/// <c>certified</c> entry's <c>proof_test</c> file exists on disk, and every
/// <c>unproven</c> entry carries a non-empty <c>rationale:</c> block.
///
/// Parallel discipline to R5.B's
/// <see cref="FailureModeManifestTests"/> — catalog-as-code, executable
/// proofs, no drift between YAML source of truth and the C# registry
/// consumed by validators.
/// </summary>
public sealed class ReplayInvariantManifestTests
{
    private static readonly string RepoRoot = FindRepoRoot();
    private static readonly string ManifestPath = Path.Combine(
        RepoRoot, "infrastructure", "observability", "certification", "replay-equivalence.yml");

    [Fact]
    public void Manifest_file_exists()
    {
        Assert.True(File.Exists(ManifestPath), $"Replay-equivalence manifest missing at {ManifestPath}.");
    }

    [Fact]
    public void Every_csharp_invariant_id_appears_in_yaml()
    {
        var yaml = File.ReadAllText(ManifestPath);
        foreach (var inv in CanonicalReplayInvariants.All)
            Assert.Contains($"id: {inv.Id}", yaml);
        foreach (var inv in CanonicalReplayInvariants.UnprovenInvariants)
            Assert.Contains($"id: {inv.Id}", yaml);
    }

    [Fact]
    public void Every_certified_invariant_has_existing_proof_test_file()
    {
        var violations = new List<string>();
        foreach (var inv in CanonicalReplayInvariants.All
                     .Where(i => i.Status == CanonicalReplayInvariants.CertifiedStatus))
        {
            if (string.IsNullOrWhiteSpace(inv.ProofTest))
            {
                violations.Add($"{inv.Id}: certified but ProofTest is null/empty.");
                continue;
            }
            var path = Path.Combine(RepoRoot, inv.ProofTest.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(path))
                violations.Add($"{inv.Id}: proof test file missing at {path}.");
        }
        Assert.True(violations.Count == 0,
            "R-REPLAY-CERTIFICATION-PROOF-EXISTS-01:\n" + string.Join("\n", violations));
    }

    [Fact]
    public void Every_unproven_invariant_has_null_proof_test_and_rationale_in_yaml()
    {
        var yaml = File.ReadAllText(ManifestPath);
        foreach (var inv in CanonicalReplayInvariants.UnprovenInvariants)
        {
            Assert.Null(inv.ProofTest);

            // Slice from the entry's id line to the next `- id:` or EOF, and
            // verify a non-empty `rationale:` block is present.
            var idx = yaml.IndexOf($"id: {inv.Id}", StringComparison.Ordinal);
            Assert.True(idx >= 0, $"{inv.Id}: id not found in YAML.");
            var nextIdx = yaml.IndexOf("  - id:", idx + 1, StringComparison.Ordinal);
            if (nextIdx < 0) nextIdx = yaml.Length;
            var slice = yaml.Substring(idx, nextIdx - idx);
            Assert.Matches(new Regex(@"rationale:\s*\|\s*\n\s+\S"), slice);
        }
    }

    [Fact]
    public void Every_status_is_either_certified_or_unproven()
    {
        var valid = new[] { CanonicalReplayInvariants.CertifiedStatus, CanonicalReplayInvariants.UnprovenStatus };
        foreach (var inv in CanonicalReplayInvariants.All)
            Assert.Contains(inv.Status, valid);
        foreach (var inv in CanonicalReplayInvariants.UnprovenInvariants)
            Assert.Equal(CanonicalReplayInvariants.UnprovenStatus, inv.Status);
    }

    [Fact]
    public void No_invariant_appears_in_both_all_and_unproven_lists()
    {
        var certified = CanonicalReplayInvariants.All.Select(i => i.Id).ToHashSet();
        var unproven = CanonicalReplayInvariants.UnprovenInvariants.Select(i => i.Id).ToHashSet();
        var overlap = certified.Intersect(unproven).ToList();
        Assert.True(overlap.Count == 0,
            "An invariant must appear in either All (certified) or UnprovenInvariants, never both: " +
            string.Join(", ", overlap));
    }

    [Fact]
    public void Every_invariant_family_is_nonempty_string()
    {
        foreach (var inv in CanonicalReplayInvariants.All.Concat(CanonicalReplayInvariants.UnprovenInvariants))
            Assert.False(string.IsNullOrWhiteSpace(inv.InvariantFamily),
                $"{inv.Id}: InvariantFamily is empty.");
    }

    [Fact]
    public void Every_invariant_lists_at_least_one_canonical_primitive()
    {
        foreach (var inv in CanonicalReplayInvariants.All.Concat(CanonicalReplayInvariants.UnprovenInvariants))
            Assert.True(inv.CanonicalPrimitives.Count > 0,
                $"{inv.Id}: CanonicalPrimitives is empty — every invariant MUST reference at least one canonical primitive.");
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
