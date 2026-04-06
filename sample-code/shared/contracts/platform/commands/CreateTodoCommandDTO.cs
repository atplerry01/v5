namespace Whycespace.Shared.Contracts.Platform.Commands;

/// <summary>
/// Flattened command DTO for creating a Todo item.
/// No domain entities exposed — primitives only.
/// </summary>
public sealed record CreateTodoCommandDTO
{
    public required string CommandId { get; init; }
    public required int Version { get; init; }
    public required DateTimeOffset Timestamp { get; init; }

    public required string Title { get; init; }
    public required string Description { get; init; }
    public int Priority { get; init; }
    public Guid? AssignedToIdentityId { get; init; }
    public string? CorrelationId { get; init; }
    public string? IdempotencyKey { get; init; }
}
