namespace Whycespace.Projections.Core.Event.EventDefinition;

public sealed record EventDefinitionView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
