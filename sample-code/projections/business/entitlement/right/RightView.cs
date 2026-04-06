namespace Whycespace.Projections.Business.Entitlement.Right;

public sealed record RightView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
