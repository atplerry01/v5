namespace Whycespace.Projections.Business.Localization.RegionalRule;

public sealed record RegionalRuleView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
