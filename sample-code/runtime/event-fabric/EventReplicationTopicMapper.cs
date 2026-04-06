namespace Whycespace.Runtime.EventFabric;

/// <summary>
/// Maps local Kafka topics to their cross-region equivalents.
/// Ensures topic naming convention compliance across regions.
///
/// Local topic:  economic.capital.vault.events
/// Remote topic: economic.capital.vault.events.replica.{source-region}
///
/// Kafka guard compliant: lowercase dot-separated naming.
/// </summary>
public sealed class EventReplicationTopicMapper
{
    private readonly string _localRegionId;

    public EventReplicationTopicMapper(string localRegionId)
    {
        _localRegionId = localRegionId;
    }

    /// <summary>
    /// Maps a local topic to its replica topic name on a remote cluster.
    /// </summary>
    public string ToReplicaTopic(string localTopic) =>
        $"{localTopic}.replica.{_localRegionId}";

    /// <summary>
    /// Extracts the source region from a replica topic name.
    /// Returns null if the topic is not a replica.
    /// </summary>
    public static string? ExtractSourceRegion(string replicaTopic)
    {
        const string marker = ".replica.";
        var idx = replicaTopic.LastIndexOf(marker, StringComparison.Ordinal);
        return idx >= 0 ? replicaTopic[(idx + marker.Length)..] : null;
    }
}
