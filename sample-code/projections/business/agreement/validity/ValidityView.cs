namespace Whycespace.Projections.Business.Agreement.Validity;

public sealed record ValidityView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
