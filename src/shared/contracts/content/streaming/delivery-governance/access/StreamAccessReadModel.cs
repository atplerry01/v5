namespace Whycespace.Shared.Contracts.Content.Streaming.DeliveryGovernance.Access;

public sealed record StreamAccessReadModel
{
    public Guid AccessId { get; init; }
    public Guid StreamId { get; init; }
    public string Mode { get; init; } = string.Empty;
    public DateTimeOffset WindowStart { get; init; }
    public DateTimeOffset WindowEnd { get; init; }
    public string Token { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string? Reason { get; init; }
    public DateTimeOffset GrantedAt { get; init; }
    public DateTimeOffset LastModifiedAt { get; init; }
}
