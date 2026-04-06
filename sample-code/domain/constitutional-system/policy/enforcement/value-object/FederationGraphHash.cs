using System.Security.Cryptography;
using System.Text;

namespace Whycespace.Domain.ConstitutionalSystem.Policy.Registry;

using Whycespace.Domain.SharedKernel;

public sealed class FederationGraphHash : ValueObject
{
    public string Value { get; }

    private FederationGraphHash(string value) => Value = value;

    public static FederationGraphHash From(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        return new FederationGraphHash(value);
    }

    public static FederationGraphHash Compute(
        IReadOnlyList<FederationNode> nodes,
        IReadOnlyList<FederationEdge> edges)
    {
        // Deterministic: sort by cluster_id -> policy_id -> version -> dependency order
        var sortedNodes = nodes
            .OrderBy(n => n.ClusterId, StringComparer.Ordinal)
            .ThenBy(n => n.PolicyId)
            .ThenBy(n => n.Version)
            .ToList();

        var sortedEdges = edges
            .OrderBy(e => e.SourcePolicyId)
            .ThenBy(e => e.TargetPolicyId)
            .ThenBy(e => e.RelationType.Value, StringComparer.Ordinal)
            .ToList();

        var sb = new StringBuilder();
        foreach (var n in sortedNodes) sb.Append($"{n.ClusterId}:{n.PolicyId}:{n.Version};");
        foreach (var e in sortedEdges) sb.Append($"{e.SourcePolicyId}->{e.TargetPolicyId}:{e.RelationType.Value};");

        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(sb.ToString()));
        var hashString = Convert.ToHexString(hash).ToLowerInvariant();

        return new FederationGraphHash(hashString);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
