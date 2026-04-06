namespace Whycespace.Platform.Api.Core.Event.EventSchema;

public sealed record EventSchemaRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record EventSchemaResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
