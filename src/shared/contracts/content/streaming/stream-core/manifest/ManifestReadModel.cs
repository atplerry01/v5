namespace Whycespace.Shared.Contracts.Content.Streaming.StreamCore.Manifest;

public sealed record ManifestReadModel
{
    public Guid ManifestId { get; init; }
    public Guid SourceId { get; init; }
    public string SourceKind { get; init; } = string.Empty;
    public int CurrentVersion { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? PublishedAt { get; init; }
    public DateTimeOffset LastModifiedAt { get; init; }
}
