namespace Whycespace.Shared.Contracts.ReadModel;

/// <summary>
/// Read-model projection DTO for Ping results.
/// Shared across platform and infrastructure — no domain dependency.
/// </summary>
public sealed record PingViewModel
{
    public required string AggregateId { get; init; }
    public required string Message { get; init; }
    public required string Status { get; init; }
    public required DateTimeOffset ProcessedAt { get; init; }
}
