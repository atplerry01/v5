namespace Whycespace.Shared.Contracts.Content.Media.CoreObject.Subtitle;

public sealed record SubtitleReadModel
{
    public Guid SubtitleId { get; init; }
    public Guid AssetRef { get; init; }
    public Guid? SourceFileRef { get; init; }
    public string Format { get; init; } = string.Empty;
    public string Language { get; init; } = string.Empty;
    public long? WindowStartMs { get; init; }
    public long? WindowEndMs { get; init; }
    public Guid? OutputRef { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset LastModifiedAt { get; init; }
    public DateTimeOffset? FinalizedAt { get; init; }
}
