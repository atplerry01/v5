namespace Whycespace.Platform.Api.Economic.Capital.Reserve;

public sealed record ReserveRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record ReserveResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
