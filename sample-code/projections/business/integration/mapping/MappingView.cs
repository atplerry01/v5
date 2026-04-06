namespace Whycespace.Projections.Business.Integration.Mapping;

public sealed record MappingView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
