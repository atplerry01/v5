namespace Whycespace.Platform.Api.Constitutional.Policy.Rule;

public sealed record RuleRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record RuleResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
