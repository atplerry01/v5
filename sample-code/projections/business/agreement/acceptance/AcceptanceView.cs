namespace Whycespace.Projections.Business.Agreement.Acceptance;

public sealed record AcceptanceView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
