using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Structural.Cluster.Provider;

public sealed record RegisterProviderCommand(
    Guid ProviderId,
    Guid ClusterReference,
    string ProviderName) : IHasAggregateId
{
    public Guid AggregateId => ProviderId;
}

public sealed record RegisterProviderWithParentCommand(
    Guid ProviderId,
    Guid ClusterReference,
    string ProviderName,
    DateTimeOffset EffectiveAt) : IHasAggregateId
{
    public Guid AggregateId => ProviderId;
}

public sealed record ActivateProviderCommand(
    Guid ProviderId) : IHasAggregateId
{
    public Guid AggregateId => ProviderId;
}

public sealed record SuspendProviderCommand(
    Guid ProviderId) : IHasAggregateId
{
    public Guid AggregateId => ProviderId;
}

public sealed record ReactivateProviderCommand(
    Guid ProviderId) : IHasAggregateId
{
    public Guid AggregateId => ProviderId;
}

public sealed record RetireProviderCommand(
    Guid ProviderId) : IHasAggregateId
{
    public Guid AggregateId => ProviderId;
}
