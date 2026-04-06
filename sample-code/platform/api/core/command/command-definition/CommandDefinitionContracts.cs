namespace Whycespace.Platform.Api.Core.Command.CommandDefinition;

public sealed record CommandDefinitionRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record CommandDefinitionResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
