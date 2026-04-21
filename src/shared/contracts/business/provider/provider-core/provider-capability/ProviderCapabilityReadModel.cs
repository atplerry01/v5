namespace Whycespace.Shared.Contracts.Business.Provider.ProviderCore.ProviderCapability;

public sealed record ProviderCapabilityReadModel
{
    public Guid ProviderCapabilityId { get; init; }
    public Guid ProviderId { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset LastUpdatedAt { get; init; }
}
