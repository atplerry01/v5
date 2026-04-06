namespace Whycespace.Platform.Api.Core.Contracts.Presentation;

/// <summary>
/// UI-ready list item presentation model.
/// Pure transformation — no enrichment beyond input data.
/// </summary>
public sealed record UiListItem
{
    public required string Id { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required string Status { get; init; }
    public required string Type { get; init; }
    public DateTimeOffset? Timestamp { get; init; }
}
