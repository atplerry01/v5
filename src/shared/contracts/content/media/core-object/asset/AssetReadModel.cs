namespace Whycespace.Shared.Contracts.Content.Media.CoreObject.Asset;

public sealed record AssetReadModel
{
    public Guid AssetId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Classification { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string Kind { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset LastModifiedAt { get; init; }
}
