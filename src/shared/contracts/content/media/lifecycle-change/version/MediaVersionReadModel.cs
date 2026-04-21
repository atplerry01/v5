namespace Whycespace.Shared.Contracts.Content.Media.LifecycleChange.Version;

public sealed record MediaVersionReadModel
{
    public Guid VersionId { get; init; }
    public Guid AssetRef { get; init; }
    public int VersionMajor { get; init; }
    public int VersionMinor { get; init; }
    public Guid FileRef { get; init; }
    public Guid? PreviousVersionId { get; init; }
    public Guid? SuccessorVersionId { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset LastModifiedAt { get; init; }
}
