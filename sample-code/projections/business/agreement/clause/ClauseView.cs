namespace Whycespace.Projections.Business.Agreement.Clause;

public sealed record ClauseView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
