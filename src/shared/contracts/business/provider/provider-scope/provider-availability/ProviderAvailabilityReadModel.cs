namespace Whycespace.Shared.Contracts.Business.Provider.ProviderScope.ProviderAvailability;

public sealed record ProviderAvailabilityReadModel
{
    public Guid ProviderAvailabilityId { get; init; }
    public Guid ProviderId { get; init; }
    public DateTimeOffset StartsAt { get; init; }
    public DateTimeOffset? EndsAt { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset LastUpdatedAt { get; init; }
}
