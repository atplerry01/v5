namespace Whycespace.Platform.Api.Core.Contracts.Access;

/// <summary>
/// Read-only access decision preview.
/// Shows what would happen if a given identity attempted a given action on a resource.
/// Advisory only — does NOT enforce or grant access.
/// Resolved via runtime adapter, not by computing policy locally.
/// </summary>
public sealed record AccessDecisionPreviewView
{
    public required string Resource { get; init; }
    public required string Action { get; init; }
    public required string Decision { get; init; }
    public required IReadOnlyList<string> Reasons { get; init; }
    public string? PolicyId { get; init; }
}
