namespace Whycespace.Shared.Contracts.Content.Media.Asset;

public sealed record MediaAssetReadModel
{
    public Guid Id { get; init; }
    public string OwnerRef { get; init; } = string.Empty;
    public int MediaType { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string ContentDigest { get; init; } = string.Empty;
    public string StorageUri { get; init; } = string.Empty;
    public long StorageSizeBytes { get; init; }
    public string Status { get; init; } = "draft";
    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();
    public DateTimeOffset? RegisteredAt { get; init; }
    public DateTimeOffset? LastTransitionedAt { get; init; }
}
