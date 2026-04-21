namespace Whycespace.Shared.Contracts.Content.Document.CoreObject.Record;

public sealed record DocumentRecordReadModel
{
    public Guid RecordId { get; init; }
    public Guid DocumentId { get; init; }
    public string Status { get; init; } = string.Empty;
    public string ClosureReason { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? ClosedAt { get; init; }
    public DateTimeOffset LastModifiedAt { get; init; }
}
