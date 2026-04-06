namespace Whycespace.Platform.Api.Core.Contracts.Presentation;

/// <summary>
/// UI-ready timeline item presentation model.
/// Represents a single event in a chronological timeline view.
/// </summary>
public sealed record UiTimelineItem
{
    public required string Id { get; init; }
    public required string Label { get; init; }
    public required DateTimeOffset Timestamp { get; init; }
    public required string Status { get; init; }
    public string? Description { get; init; }
    public string? Category { get; init; }
}
