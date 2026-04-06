namespace Whycespace.Platform.Api.Business.Integration.Adapter;

public sealed record AdapterRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record AdapterResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
