using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Business.Provider.ProviderCore.ProviderTier;

public sealed record CreateProviderTierCommand(
    Guid ProviderTierId,
    string Code,
    string Name,
    int Rank) : IHasAggregateId
{
    public Guid AggregateId => ProviderTierId;
}

public sealed record UpdateProviderTierCommand(
    Guid ProviderTierId,
    string Name,
    int Rank) : IHasAggregateId
{
    public Guid AggregateId => ProviderTierId;
}

public sealed record ActivateProviderTierCommand(Guid ProviderTierId) : IHasAggregateId
{
    public Guid AggregateId => ProviderTierId;
}

public sealed record ArchiveProviderTierCommand(Guid ProviderTierId) : IHasAggregateId
{
    public Guid AggregateId => ProviderTierId;
}
