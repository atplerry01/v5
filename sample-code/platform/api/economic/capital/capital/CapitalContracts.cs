namespace Whycespace.Platform.Api.Economic.Capital.Capital;

public sealed record CapitalRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record CapitalResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
