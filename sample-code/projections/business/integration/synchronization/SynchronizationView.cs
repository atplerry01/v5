namespace Whycespace.Projections.Business.Integration.Synchronization;

public sealed record SynchronizationView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
