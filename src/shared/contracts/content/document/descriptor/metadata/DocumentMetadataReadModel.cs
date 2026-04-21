namespace Whycespace.Shared.Contracts.Content.Document.Descriptor.Metadata;

public sealed record DocumentMetadataReadModel
{
    public Guid MetadataId { get; init; }
    public Guid DocumentId { get; init; }
    public string Status { get; init; } = string.Empty;
    public IReadOnlyDictionary<string, string> Entries { get; init; } =
        new Dictionary<string, string>();
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset LastModifiedAt { get; init; }
    public DateTimeOffset? FinalizedAt { get; init; }
}
