using System.Collections.Concurrent;

namespace Whycespace.Projections.PolicyFederation;

/// <summary>
/// Federation graph projection -- caches resolved evaluation order and graph metadata.
/// Read-only projection. No writes to source data.
/// Key: federation_graph:{hash}
/// Invalidated on new version.
/// </summary>
public sealed class FederationGraphProjection
{
    private readonly ConcurrentDictionary<string, FederationGraphCacheEntry> _cache = new();

    public FederationGraphCacheEntry? Get(string graphHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(graphHash);
        _cache.TryGetValue(FormatKey(graphHash), out var entry);
        return entry;
    }

    public void Set(string graphHash, FederationGraphCacheEntry entry)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(graphHash);
        ArgumentNullException.ThrowIfNull(entry);
        _cache[FormatKey(graphHash)] = entry;
    }

    public void Invalidate(string graphHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(graphHash);
        _cache.TryRemove(FormatKey(graphHash), out _);
    }

    public void InvalidateAll() => _cache.Clear();

    public bool Contains(string graphHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(graphHash);
        return _cache.ContainsKey(FormatKey(graphHash));
    }

    private static string FormatKey(string graphHash) => $"federation_graph:{graphHash}";
}

public sealed record FederationGraphCacheEntry(
    string GraphHash,
    IReadOnlyList<string> ResolvedOrder,
    int NodeCount,
    int EdgeCount,
    DateTimeOffset CachedAt);
