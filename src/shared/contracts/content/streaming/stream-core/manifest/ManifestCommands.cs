using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Content.Streaming.StreamCore.Manifest;

public sealed record CreateManifestCommand(
    Guid ManifestId,
    Guid SourceId,
    string SourceKind,
    DateTimeOffset CreatedAt) : IHasAggregateId
{
    public Guid AggregateId => ManifestId;
}

public sealed record UpdateManifestCommand(
    Guid ManifestId,
    DateTimeOffset UpdatedAt) : IHasAggregateId
{
    public Guid AggregateId => ManifestId;
}

public sealed record PublishManifestCommand(
    Guid ManifestId,
    DateTimeOffset PublishedAt) : IHasAggregateId
{
    public Guid AggregateId => ManifestId;
}

public sealed record RetireManifestCommand(
    Guid ManifestId,
    string Reason,
    DateTimeOffset RetiredAt) : IHasAggregateId
{
    public Guid AggregateId => ManifestId;
}

public sealed record ArchiveManifestCommand(
    Guid ManifestId,
    DateTimeOffset ArchivedAt) : IHasAggregateId
{
    public Guid AggregateId => ManifestId;
}
