namespace Whycespace.Platform.Api.Business.Integration.Schema;

public sealed record SchemaRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record SchemaResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
