namespace Whycespace.Shared.Contracts.Content.Streaming.DeliveryGovernance.EntitlementHook;

public sealed record EntitlementHookReadModel
{
    public Guid HookId { get; init; }
    public Guid TargetId { get; init; }
    public string SourceSystem { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string? FailureReason { get; init; }
    public DateTimeOffset? LastCheckedAt { get; init; }
    public DateTimeOffset RegisteredAt { get; init; }
    public DateTimeOffset LastModifiedAt { get; init; }
}
