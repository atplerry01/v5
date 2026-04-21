namespace Whycespace.Shared.Contracts.Content.Document.Intake.Upload;

public sealed record DocumentUploadReadModel
{
    public Guid UploadId { get; init; }
    public Guid SourceRef { get; init; }
    public Guid InputRef { get; init; }
    public Guid? OutputRef { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? FailureReason { get; init; }
    public DateTimeOffset RequestedAt { get; init; }
    public DateTimeOffset? AcceptedAt { get; init; }
    public DateTimeOffset? StartedAt { get; init; }
    public DateTimeOffset? CompletedAt { get; init; }
    public DateTimeOffset LastModifiedAt { get; init; }
}
