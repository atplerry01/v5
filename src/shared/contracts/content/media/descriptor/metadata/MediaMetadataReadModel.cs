namespace Whycespace.Shared.Contracts.Content.Media.Descriptor.Metadata;

public sealed record MediaMetadataReadModel
{
    public Guid MetadataId { get; init; }
    public Guid AssetRef { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset LastModifiedAt { get; init; }
    public DateTimeOffset? FinalizedAt { get; init; }
}
