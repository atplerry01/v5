namespace Whycespace.Shared.Contracts.ReadModel;

/// <summary>
/// Read-model projection DTO for Todo items.
/// Shared across platform and infrastructure — no domain dependency.
/// </summary>
public sealed record TodoViewModel
{
    public required Guid TodoId { get; init; }
    public required string Title { get; init; }
    public required string Status { get; init; }
    public required int Priority { get; init; }
    public string? AssignedTo { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public required DateTimeOffset UpdatedAt { get; init; }
}
