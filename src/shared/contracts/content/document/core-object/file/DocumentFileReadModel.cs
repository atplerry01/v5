namespace Whycespace.Shared.Contracts.Content.Document.CoreObject.File;

public sealed record DocumentFileReadModel
{
    public Guid DocumentFileId { get; init; }
    public Guid DocumentId { get; init; }
    public string StorageRef { get; init; } = string.Empty;
    public string DeclaredChecksum { get; init; } = string.Empty;
    public string MimeType { get; init; } = string.Empty;
    public long SizeBytes { get; init; }
    public string IntegrityStatus { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public Guid? SuccessorFileId { get; init; }
    public DateTimeOffset RegisteredAt { get; init; }
    public DateTimeOffset LastModifiedAt { get; init; }
}
