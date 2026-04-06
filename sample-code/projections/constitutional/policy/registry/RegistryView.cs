namespace Whycespace.Projections.Constitutional.Policy.Registry;

public sealed record RegistryView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
