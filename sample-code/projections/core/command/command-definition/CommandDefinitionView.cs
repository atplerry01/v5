namespace Whycespace.Projections.Core.Command.CommandDefinition;

public sealed record CommandDefinitionView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
