namespace Whycespace.Platform.Api.Orchestration.Workflow.Template;

public sealed record TemplateRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record TemplateResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
