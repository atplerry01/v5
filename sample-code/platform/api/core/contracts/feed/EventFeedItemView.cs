namespace Whycespace.Platform.Api.Core.Contracts.Feed;

/// <summary>
/// Read-only view of an event feed item.
/// Sourced from projections — never raw event payloads.
/// Categories: WORKFLOW, ECONOMIC, GOVERNANCE, ALERT.
/// </summary>
public sealed record EventFeedItemView
{
    public required string EventId { get; init; }
    public required string EventType { get; init; }
    public required string Category { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required DateTime Timestamp { get; init; }
}
