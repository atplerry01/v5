using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Business.Provider.ProviderGovernance.ProviderAgreement;

public sealed record CreateProviderAgreementCommand(
    Guid ProviderAgreementId,
    Guid ProviderId,
    Guid? ContractId,
    DateTimeOffset? EffectiveStartsAt,
    DateTimeOffset? EffectiveEndsAt) : IHasAggregateId
{
    public Guid AggregateId => ProviderAgreementId;
}

public sealed record ActivateProviderAgreementCommand(
    Guid ProviderAgreementId,
    DateTimeOffset EffectiveStartsAt,
    DateTimeOffset? EffectiveEndsAt) : IHasAggregateId
{
    public Guid AggregateId => ProviderAgreementId;
}

public sealed record SuspendProviderAgreementCommand(
    Guid ProviderAgreementId,
    DateTimeOffset SuspendedAt) : IHasAggregateId
{
    public Guid AggregateId => ProviderAgreementId;
}

public sealed record TerminateProviderAgreementCommand(
    Guid ProviderAgreementId,
    DateTimeOffset TerminatedAt) : IHasAggregateId
{
    public Guid AggregateId => ProviderAgreementId;
}
