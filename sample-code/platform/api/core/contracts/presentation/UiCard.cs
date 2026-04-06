namespace Whycespace.Platform.Api.Core.Contracts.Presentation;

/// <summary>
/// UI-ready card presentation model.
/// Pure transformation from domain view models — no business logic.
/// Supports status types: SUCCESS, FAILED, RUNNING, PENDING, COMPLETED.
/// </summary>
public sealed record UiCard
{
    public required string Id { get; init; }
    public required string Title { get; init; }
    public required string Subtitle { get; init; }
    public required string Status { get; init; }
    public required string Type { get; init; }
    public DateTimeOffset? Timestamp { get; init; }
    public IReadOnlyDictionary<string, string>? Tags { get; init; }
}
