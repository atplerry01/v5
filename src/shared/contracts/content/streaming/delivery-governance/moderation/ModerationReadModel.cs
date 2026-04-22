namespace Whycespace.Shared.Contracts.Content.Streaming.DeliveryGovernance.Moderation;

public sealed record ModerationReadModel
{
    public Guid ModerationId { get; init; }
    public Guid TargetId { get; init; }
    public string FlagReason { get; init; } = string.Empty;
    public Guid? ModeratorId { get; init; }
    public string? Decision { get; init; }
    public string? Rationale { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset FlaggedAt { get; init; }
    public DateTimeOffset LastModifiedAt { get; init; }
}
