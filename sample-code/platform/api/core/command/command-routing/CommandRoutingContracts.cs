namespace Whycespace.Platform.Api.Core.Command.CommandRouting;

public sealed record CommandRoutingRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record CommandRoutingResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
