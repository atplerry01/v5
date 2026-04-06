namespace Whycespace.Projections.Business.Integration.Adapter;

public sealed record AdapterView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
