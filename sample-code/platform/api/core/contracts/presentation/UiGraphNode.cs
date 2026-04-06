namespace Whycespace.Platform.Api.Core.Contracts.Presentation;

/// <summary>
/// UI-ready graph node for visual rendering.
/// Stripped of internal system data — only visual-safe properties.
/// </summary>
public sealed record UiGraphNode
{
    public required string Id { get; init; }
    public required string Label { get; init; }
    public required string Type { get; init; }
    public required string Status { get; init; }
    public string? Group { get; init; }
}
