namespace Whycespace.Shared.Contracts.Structural.Structure.Classification;

public sealed record ClassificationReadModel
{
    public Guid ClassificationId { get; init; }
    public string ClassificationName { get; init; } = string.Empty;
    public string ClassificationCategory { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset LastModifiedAt { get; init; }
}
