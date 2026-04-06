namespace Whycespace.Projections.Business.Agreement.Term;

public sealed record TermView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
