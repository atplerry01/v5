namespace Whycespace.Shared.Contracts.Content.Media.TechnicalProcessing.Processing;

public sealed record MediaProcessingReadModel
{
    public Guid JobId { get; init; }
    public string Kind { get; init; } = string.Empty;
    public Guid InputRef { get; init; }
    public Guid? OutputRef { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? FailureReason { get; init; }
    public DateTimeOffset RequestedAt { get; init; }
    public DateTimeOffset? StartedAt { get; init; }
    public DateTimeOffset? CompletedAt { get; init; }
    public DateTimeOffset LastModifiedAt { get; init; }
}
