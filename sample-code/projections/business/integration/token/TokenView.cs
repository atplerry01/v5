namespace Whycespace.Projections.Business.Integration.Token;

public sealed record TokenView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
