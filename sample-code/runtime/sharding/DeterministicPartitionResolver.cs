namespace Whycespace.Runtime.Sharding;

/// <summary>
/// FNV-1a based deterministic partition and shard resolver.
/// Same input ALWAYS produces the same partition/shard — across nodes, across restarts.
///
/// Coexists with:
/// - PartitionKeyResolver (resolves WHICH key to use from envelope)
/// - WorkflowRouter (SHA256-based node affinity for workflows)
///
/// This resolver maps a resolved key to a numeric partition and shard ID.
/// </summary>
public static class DeterministicPartitionResolver
{
    /// <summary>
    /// Resolves a deterministic partition index using FNV-1a hash.
    /// </summary>
    public static int ResolvePartition(string key, int partitionCount)
    {
        ArgumentNullException.ThrowIfNull(key);
        if (partitionCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(partitionCount), "Partition count must be positive.");

        unchecked
        {
            uint hash = 2166136261; // FNV offset basis

            foreach (var c in key)
            {
                hash ^= c;
                hash *= 16777619; // FNV prime
            }

            return (int)(hash % (uint)partitionCount);
        }
    }

    /// <summary>
    /// Resolves a deterministic shard identifier from a key.
    /// </summary>
    public static string ResolveShardId(string key, int shardCount)
    {
        var shard = ResolvePartition(key, shardCount);
        return $"shard-{shard}";
    }
}
