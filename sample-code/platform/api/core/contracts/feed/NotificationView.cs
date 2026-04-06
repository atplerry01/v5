namespace Whycespace.Platform.Api.Core.Contracts.Feed;

/// <summary>
/// Read-only view of a user notification.
/// Sourced from projections — never raw event payloads.
/// Types: INFO, WARNING, CRITICAL.
/// </summary>
public sealed record NotificationView
{
    public required string NotificationId { get; init; }
    public required string Type { get; init; }
    public required string Message { get; init; }
    public required bool IsRead { get; init; }
    public required DateTime CreatedAt { get; init; }
}
