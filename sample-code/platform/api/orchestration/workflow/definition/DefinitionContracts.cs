namespace Whycespace.Platform.Api.Orchestration.Workflow.Definition;

public sealed record DefinitionRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record DefinitionResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
