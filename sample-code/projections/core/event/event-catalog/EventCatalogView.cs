namespace Whycespace.Projections.Core.Event.EventCatalog;

public sealed record EventCatalogView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
