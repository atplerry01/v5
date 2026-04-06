namespace Whycespace.Platform.Api.Core.Event.EventDefinition;

public sealed record EventDefinitionRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record EventDefinitionResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
