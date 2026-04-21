namespace Whycespace.Shared.Contracts.Content.Document.LifecycleChange.Version;

public sealed record DocumentVersionReadModel
{
    public Guid VersionId { get; init; }
    public Guid DocumentRef { get; init; }
    public int Major { get; init; }
    public int Minor { get; init; }
    public Guid ArtifactRef { get; init; }
    public Guid? PreviousVersionId { get; init; }
    public Guid? SuccessorVersionId { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? WithdrawalReason { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset LastModifiedAt { get; init; }
}
