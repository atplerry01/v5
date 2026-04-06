using System.Collections.Concurrent;

namespace Whycespace.Projections.PolicyFederation;

/// <summary>
/// Caches dependency chains per policy within a federation graph.
/// Key: policy_id -> dependency_chain
/// Invalidated when federation graph version changes.
/// </summary>
public sealed class FederationDependencyCache
{
    private readonly ConcurrentDictionary<string, IReadOnlyList<string>> _cache = new();

    public IReadOnlyList<string>? GetDependencyChain(string graphHash, Guid policyId)
    {
        var key = FormatKey(graphHash, policyId);
        _cache.TryGetValue(key, out var chain);
        return chain;
    }

    public void SetDependencyChain(string graphHash, Guid policyId, IReadOnlyList<string> chain)
    {
        ArgumentNullException.ThrowIfNull(chain);
        var key = FormatKey(graphHash, policyId);
        _cache[key] = chain;
    }

    public void InvalidateGraph(string graphHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(graphHash);
        var keysToRemove = _cache.Keys
            .Where(k => k.StartsWith($"federation_dep:{graphHash}:", StringComparison.Ordinal))
            .ToList();

        foreach (var key in keysToRemove)
            _cache.TryRemove(key, out _);
    }

    public void InvalidateAll() => _cache.Clear();

    private static string FormatKey(string graphHash, Guid policyId) =>
        $"federation_dep:{graphHash}:{policyId}";
}
