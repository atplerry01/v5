namespace Whycespace.Shared.Contracts.Platform.Routing.DispatchRule;

public sealed record DispatchRuleReadModel
{
    public Guid DispatchRuleId { get; init; }
    public string RuleName { get; init; } = string.Empty;
    public Guid RouteRef { get; init; }
    public string ConditionType { get; init; } = string.Empty;
    public string MatchValue { get; init; } = string.Empty;
    public int Priority { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset LastModifiedAt { get; init; }
}
