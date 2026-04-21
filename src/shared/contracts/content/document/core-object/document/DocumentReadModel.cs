namespace Whycespace.Shared.Contracts.Content.Document.CoreObject.Document;

public sealed record DocumentReadModel
{
    public Guid DocumentId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public string Classification { get; init; } = string.Empty;
    public Guid StructuralOwnerId { get; init; }
    public string BusinessOwnerKind { get; init; } = string.Empty;
    public Guid BusinessOwnerId { get; init; }
    public Guid? CurrentVersionId { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset LastModifiedAt { get; init; }
}
