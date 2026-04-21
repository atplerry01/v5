namespace Whycespace.Shared.Contracts.Business.Provider.ProviderCore.ProviderTier;

public sealed record ProviderTierReadModel
{
    public Guid ProviderTierId { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public int Rank { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset LastUpdatedAt { get; init; }
}
