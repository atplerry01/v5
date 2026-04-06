namespace Whycespace.Platform.Api.Business.Integration.Gateway;

public sealed record GatewayRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record GatewayResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
