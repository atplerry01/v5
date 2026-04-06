namespace Whycespace.Platform.Api.Business.Localization.RegionalRule;

public sealed record RegionalRuleRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record RegionalRuleResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
