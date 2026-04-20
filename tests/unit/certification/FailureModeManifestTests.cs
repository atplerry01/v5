using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Whycespace.Tests.Unit.Certification;

/// <summary>
/// R5.B / R-CHAOS-FAILURE-MODE-REGISTRY-01 — verifies the failure-mode
/// catalog YAML exists, parses structurally, is in sync with the C# mirror
/// <see cref="CanonicalFailureModes"/>, and every <c>certified</c> entry's
/// <c>proof_test</c> file actually exists on disk.
/// </summary>
public sealed class FailureModeManifestTests
{
    private static readonly string RepoRoot = FindRepoRoot();
    private static readonly string ManifestPath = Path.Combine(
        RepoRoot, "infrastructure", "observability", "certification", "runtime-failure-modes.yml");

    [Fact]
    public void Manifest_file_exists()
    {
        Assert.True(File.Exists(ManifestPath), $"Failure-mode manifest missing at {ManifestPath}.");
    }

    [Fact]
    public void Every_failure_mode_id_in_csharp_mirror_appears_in_yaml()
    {
        var yaml = File.ReadAllText(ManifestPath);
        foreach (var fm in CanonicalFailureModes.All)
        {
            Assert.Contains($"id: {fm.Id}", yaml);
        }
    }

    [Fact]
    public void Every_status_is_either_certified_or_unproven()
    {
        var valid = new[] { CanonicalFailureModes.CertifiedStatus, CanonicalFailureModes.UnprovenStatus };
        foreach (var fm in CanonicalFailureModes.All)
        {
            Assert.Contains(fm.Status, valid);
        }
    }

    [Fact]
    public void Every_certified_failure_mode_has_existing_proof_test_file()
    {
        var violations = new List<string>();
        foreach (var fm in CanonicalFailureModes.All
                     .Where(m => m.Status == CanonicalFailureModes.CertifiedStatus))
        {
            if (string.IsNullOrWhiteSpace(fm.ProofTest))
            {
                violations.Add($"{fm.Id}: certified entry but ProofTest is null/empty.");
                continue;
            }
            var path = Path.Combine(RepoRoot, fm.ProofTest.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(path))
                violations.Add($"{fm.Id}: proof test file missing at {path}.");
        }
        Assert.True(violations.Count == 0,
            "R-CHAOS-PROOF-EXISTS-01:\n" + string.Join("\n", violations));
    }

    [Fact]
    public void Every_unproven_failure_mode_has_null_proof_test_and_nonempty_rationale_in_yaml()
    {
        var yaml = File.ReadAllText(ManifestPath);
        foreach (var fm in CanonicalFailureModes.All
                     .Where(m => m.Status == CanonicalFailureModes.UnprovenStatus))
        {
            Assert.Null(fm.ProofTest);
            // Rationale lives only in the YAML — slice the entry and verify a
            // non-empty rationale block. We locate the entry by its id and
            // scan forward until the next `- id:` line or EOF.
            var idIdx = yaml.IndexOf($"id: {fm.Id}", StringComparison.Ordinal);
            Assert.True(idIdx >= 0, $"{fm.Id}: not found in YAML.");
            var nextIdIdx = yaml.IndexOf("  - id:", idIdx + 1, StringComparison.Ordinal);
            if (nextIdIdx < 0) nextIdIdx = yaml.IndexOf("operational_only_alerts:", idIdx, StringComparison.Ordinal);
            if (nextIdIdx < 0) nextIdIdx = yaml.Length;
            var slice = yaml.Substring(idIdx, nextIdIdx - idIdx);
            Assert.Matches(new Regex(@"rationale:\s*\|\s*\n\s+\S"), slice);
        }
    }

    [Fact]
    public void Operational_only_alerts_csharp_mirror_is_in_sync_with_yaml()
    {
        var yaml = File.ReadAllText(ManifestPath);
        foreach (var op in CanonicalFailureModes.OperationalOnlyAlerts)
        {
            Assert.Contains($"alert: {op.Alert}", yaml);
        }
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
