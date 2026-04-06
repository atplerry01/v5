namespace Whycespace.Shared.Contracts.Infrastructure.Storage;

/// <summary>
/// Contract for identity event store stream configuration.
/// Implementations provide domain-specific aggregate type lists and metadata resolution.
/// </summary>
public interface IIdentityEventStoreConfig
{
    IReadOnlyList<string> AggregateTypes { get; }
    IdentityStreamMetadata GetMetadata(string aggregateType);
    string BuildStreamId(string aggregateType, Guid aggregateId);
}

/// <summary>
/// Stream metadata for identity event store partitioning.
/// </summary>
public sealed record IdentityStreamMetadata(
    string Cluster,
    string SubCluster,
    string App,
    string Context);
