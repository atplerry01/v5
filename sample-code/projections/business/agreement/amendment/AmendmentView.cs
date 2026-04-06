namespace Whycespace.Projections.Business.Agreement.Amendment;

public sealed record AmendmentView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
