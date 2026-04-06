namespace Whycespace.Projections.Core.Command.CommandCatalog;

public sealed record CommandCatalogView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
