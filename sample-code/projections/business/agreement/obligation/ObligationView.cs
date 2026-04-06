namespace Whycespace.Projections.Business.Agreement.Obligation;

public sealed record ObligationView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
