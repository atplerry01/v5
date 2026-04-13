using System.Text.RegularExpressions;
using Whycespace.Engines.T0U.Determinism.Time;
using Whycespace.Shared.Kernel.Determinism;

namespace Whycespace.Engines.T0U.Determinism;

/// <summary>
/// HSID v2.1 — single source of truth for compact, topology-aware,
/// bucket-scoped, bounded-sequence IDs of the shape
/// <c>PPP-LLLL-TTT-TOPOLOGY-SEQ</c>.
///
/// Width contract (LOCKED 2026-04-07):
/// <list type="bullet">
///   <item>PPP — 3 chars (enum name)</item>
///   <item>LLLL — 4 chars</item>
///   <item>TTT — 3 chars (uppercase hex)</item>
///   <item>TOPOLOGY — 12 chars (Cluster 3 + SubCluster 3 + SPV 6)</item>
///   <item>SEQ — 3 chars (uppercase hex, X3)</item>
/// </list>
/// </summary>
public sealed class DeterministicIdEngine : IDeterministicIdEngine
{
    // LLLL accepts [A-Z0-9]{4}: location codes may be alpha mnemonics (e.g.
    // UKED, NGLG) OR hex-derived 4-char prefixes (e.g. EAF8) when no
    // authoritative location mnemonic is supplied. Both are width-stable and
    // deterministic; the prior alpha-only constraint conflicted with
    // RuntimeControlPlane.DeriveLocation which hashes TenantId → hex.
    private static readonly Regex Format = new(
        "^[A-Z]{3}-[A-Z0-9]{4}-[A-Z0-9]{3}-[A-Z0-9]{12}-[A-Z0-9]{3}$",
        RegexOptions.Compiled);

    private readonly ITimeBucketProvider _bucketProvider;

    public DeterministicIdEngine(ITimeBucketProvider bucketProvider)
    {
        _bucketProvider = bucketProvider;
    }

    public string Generate(
        IdPrefix prefix,
        LocationCode location,
        TopologyCode topology,
        string seed,
        int sequence)
    {
        if (string.IsNullOrEmpty(seed))
            throw new ArgumentException("HSID seed must be non-empty.", nameof(seed));

        var locationStr = location.ToString();
        if (locationStr.Length != 4)
            throw new ArgumentException(
                $"LocationCode must be 4 chars, got '{locationStr}'.", nameof(location));

        var topologyStr = topology.ToString();
        if (topologyStr.Length != 12)
            throw new ArgumentException(
                $"TopologyCode must be 12 chars (CCC+XXX+SSSSSS), got '{topologyStr}'.", nameof(topology));

        var bucket = _bucketProvider.GetBucket(seed);
        return $"{prefix}-{locationStr}-{bucket}-{topologyStr}-{sequence:X3}";
    }

    public bool IsValid(string id)
    {
        if (string.IsNullOrEmpty(id)) return false;
        if (!Format.IsMatch(id)) return false;

        // Per-segment width revalidation (H5 strict structural check). Width
        // is locked at X3 for both bucket and sequence per the canonical
        // deterministic-id.guard.md G18.
        var parts = id.Split('-');
        if (parts.Length != 5) return false;
        if (parts[0].Length != 3) return false;   // PPP
        if (parts[1].Length != 4) return false;   // LLLL
        if (parts[2].Length != 3) return false;   // TTT (X3 locked)
        if (parts[3].Length != 12) return false;  // TOPOLOGY
        if (parts[4].Length != 3) return false;   // SEQ (X3 locked)

        // Prefix must be a known IdPrefix enum name (no caller-invented prefixes).
        if (!Enum.TryParse<IdPrefix>(parts[0], ignoreCase: false, out _)) return false;

        return true;
    }
}
