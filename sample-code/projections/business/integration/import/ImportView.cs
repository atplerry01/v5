namespace Whycespace.Projections.Business.Integration.Import;

public sealed record ImportView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
