namespace Whycespace.Projections.Business.Integration.Provider;

public sealed record ProviderView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
