namespace Whycespace.Projections.Business.Integration.Schema;

public sealed record SchemaView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
