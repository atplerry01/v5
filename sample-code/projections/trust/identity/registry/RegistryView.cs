namespace Whycespace.Projections.Trust.Identity.Registry;

public sealed record RegistryView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
