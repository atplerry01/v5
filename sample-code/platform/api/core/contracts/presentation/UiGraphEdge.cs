namespace Whycespace.Platform.Api.Core.Contracts.Presentation;

/// <summary>
/// UI-ready graph edge for visual rendering.
/// Directional relationship between two nodes.
/// </summary>
public sealed record UiGraphEdge
{
    public required string From { get; init; }
    public required string To { get; init; }
    public required string Type { get; init; }
    public string? Label { get; init; }
}
