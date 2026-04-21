using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Business.Provider.ProviderCore.ProviderCapability;

public sealed record CreateProviderCapabilityCommand(
    Guid ProviderCapabilityId,
    Guid ProviderId,
    string Code,
    string Name) : IHasAggregateId
{
    public Guid AggregateId => ProviderCapabilityId;
}

public sealed record UpdateProviderCapabilityCommand(
    Guid ProviderCapabilityId,
    string Name) : IHasAggregateId
{
    public Guid AggregateId => ProviderCapabilityId;
}

public sealed record ActivateProviderCapabilityCommand(Guid ProviderCapabilityId) : IHasAggregateId
{
    public Guid AggregateId => ProviderCapabilityId;
}

public sealed record ArchiveProviderCapabilityCommand(Guid ProviderCapabilityId) : IHasAggregateId
{
    public Guid AggregateId => ProviderCapabilityId;
}
