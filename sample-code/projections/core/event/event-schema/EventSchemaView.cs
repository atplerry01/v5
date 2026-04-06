namespace Whycespace.Projections.Core.Event.EventSchema;

public sealed record EventSchemaView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
