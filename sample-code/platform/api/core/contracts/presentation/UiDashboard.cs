namespace Whycespace.Platform.Api.Core.Contracts.Presentation;

/// <summary>
/// Composite UI dashboard response.
/// Aggregates cards, timeline items, list items, and graph data
/// into a single presentation-ready structure.
/// </summary>
public sealed record UiDashboard
{
    public IReadOnlyList<UiCard> Cards { get; init; } = [];
    public IReadOnlyList<UiTimelineItem> Timeline { get; init; } = [];
    public IReadOnlyList<UiListItem> Items { get; init; } = [];
    public UiGraphData? Graph { get; init; }
}

/// <summary>
/// UI-ready graph data combining nodes and edges.
/// </summary>
public sealed record UiGraphData
{
    public required IReadOnlyList<UiGraphNode> Nodes { get; init; }
    public required IReadOnlyList<UiGraphEdge> Edges { get; init; }
}
